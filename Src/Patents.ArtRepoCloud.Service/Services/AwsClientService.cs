using Patents.ArtRepoCloud.Service.Services.Interfaces;
using Patents.ArtRepoCloud.Service.AWS;
using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.Runtime;
using Patents.ArtRepoCloud.Service.Configuration;
using NLog;
using System.Net;

namespace Patents.ArtRepoCloud.Service.Services
{
    public class AwsClientService : IAwsClientService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly RegionEndpoint[] AllowedRegions =
        {
            RegionEndpoint.USEast1,
            RegionEndpoint.USEast2,
            RegionEndpoint.USWest1,
            RegionEndpoint.USWest2
        };

        private static readonly string[] PendingStatusNames = new[] { "running" };
        private static readonly string[] StoppedStatusNames = new[] { "stopped" };

        private readonly Dictionary<RegionEndpoint, IAmazonEC2> _clients = new Dictionary<RegionEndpoint, IAmazonEC2>();

        private List<IProxyInstance> _allInstances;
        private readonly AppSettings _appSettings;
        private readonly AwsSettings _awsSettings;

        public AwsClientService(AppSettings appSettings, AwsSettings awsSettings)
        {
            _appSettings = appSettings;
            _awsSettings = awsSettings;

            _allInstances = new List<IProxyInstance>();

            var creds = new BasicAWSCredentials(_awsSettings.AwsAccessKey, _awsSettings.AwsSecretKey);

            foreach (var region in AllowedRegions)
            {
                _clients.Add(region, new AmazonEC2Client(creds, region));
            }
        }

        public async Task<List<IProxyInstance>> GetAllInstances(CancellationToken cancellationToken = default)
        {
            if (!_allInstances.Any())
            {
                foreach (var client in _clients)
                {
                    Log.Debug("Getting grabbers from region {0}", client.Key);

                    var instances = await client.Value.DescribeInstancesAsync(cancellationToken);

                    var resevervations =
                        instances.Reservations.Where(x =>
                            x.Instances.Any(y =>
                                y.Tags.Any(z =>
                                    z.Key == "PAIREnvironment" &&
                                    z.Value == _appSettings.Environment
                                ) &&
                                y.Tags.Any(z => z.Key == "ScraperId")
                                )
                            ).ToList();

                    Log.Debug("Found {0} grabbers in region {1} with Tags: PAIREnvironment = {2} and a ScraperId tag", resevervations.Count, client.Key, _appSettings.Environment);

                    foreach (var reservation in resevervations)
                    {
                        foreach (var instance in reservation.Instances)
                        {
                            if (
                                !Guid.TryParse(
                                    instance.Tags.Where(x => x.Key.Equals("ScraperId")).Select(x => x.Value).FirstOrDefault(),
                                    out Guid id))
                            {
                                Log.Error(
                                    "Amazon instance {0} in region {1} does not have a valid ScraperId. The ID should be a GUID in a tag named ScraperId.",
                                    instance.InstanceId, instance.Placement.AvailabilityZone);
                            }

                            else
                            {
                                Log.Debug("Found amazon grabber {0} in region {1}. State is: {2}", instance.InstanceId, client.Key, instance.State.Name.Value);

                                _allInstances.Add(
                                    new ProxyInstance(
                                        id,
                                        instance.InstanceId,
                                        PendingStatusNames.Contains(instance.State.Name.Value),
                                        instance.PublicIpAddress,
                                        instance.PrivateIpAddress,
                                        client.Key)
                                    );
                            }
                        }
                    }
                }
            }

            return _allInstances;
        }

        public async Task<IProxyInstance> GetAvailableInstance(CancellationToken cancellationToken = default)
        {
            var instances = await GetAllInstances(cancellationToken).ConfigureAwait(false);

            var availableInstance = instances.FirstOrDefault(i => i.IsAvailable && i.Running) ?? instances.FirstOrDefault(i => i.IsAvailable && !i.Running);

            if (!availableInstance.Running)
            {
                await Start(availableInstance, cancellationToken).ConfigureAwait(false);
            }

            if (availableInstance.Running)
            {
                availableInstance.SetStatus(false);

                return availableInstance;
            }

            return null;
        }

        public Task<IInstance> GetInstance(Guid id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Start(IInstance grabber, CancellationToken cancellationToken)
        {
            Log.Info("Starting grabber {0}", grabber);
            var req = new StartInstancesRequest();
            req.InstanceIds.Add(grabber.InstanceId);

            do
            {
                var request = new DescribeInstanceStatusRequest();
                request.InstanceIds = new List<string>();
                request.IncludeAllInstances = true;
                request.InstanceIds.Add(grabber.InstanceId);

                var status = await _clients[grabber.Endpoint].DescribeInstanceStatusAsync(request, cancellationToken);

                var startWaitTime = DateTime.Now;
                var instance = status.InstanceStatuses.FirstOrDefault(x => x.InstanceId == grabber.InstanceId);
                if (instance == null)
                {
                    Log.Error("AWS could not find grabber {0} after a DescribeInstanceStatusRequest.", grabber);
                    return false;
                }

                if (PendingStatusNames.Contains(instance.InstanceState.Name.Value))
                {
                    Log.Info("AWS grabber {0} is already started.", grabber);
                    return true;
                }

                if (!StoppedStatusNames.Contains(instance.InstanceState.Name.Value))
                {

                    if (DateTime.Now - startWaitTime < TimeSpan.FromMinutes(1))
                    {
                        Log.Debug("Waiting for AWS grabber {0} to become startable.", grabber);
                        Thread.Sleep(5000);
                    }

                    else
                    {
                        Log.Warn("AWS instance was not startable after 60 seconds.");
                        return false;
                    }
                }
                else
                {
                    break;
                }

            } while (true);


            var result = await _clients[grabber.Endpoint].StartInstancesAsync(req, cancellationToken);
            if (result.HttpStatusCode != HttpStatusCode.OK)
            {
                Log.Error("AWS reported a non-success error code for grabber {0}", grabber);
                return false;
            }

            // Wait fo grabber to start.
            do
            {
                var request = new DescribeInstanceStatusRequest();
                request.InstanceIds = new List<string>();
                request.IncludeAllInstances = true;
                request.InstanceIds.Add(grabber.InstanceId);

                var status = await _clients[grabber.Endpoint].DescribeInstanceStatusAsync(request, cancellationToken);

                var startWaitTime = DateTime.Now;
                var instance = status.InstanceStatuses.FirstOrDefault(x => x.InstanceId == grabber.InstanceId);
                if (instance == null)
                {
                    Log.Error("AWS could not find grabber {0} after a DescribeInstanceStatusRequest.", grabber);
                    return false;
                }

                if (PendingStatusNames.Contains(instance.InstanceState.Name.Value))
                {
                    Log.Info("AWS grabber {0} is now started.", grabber);
                    grabber.SetRunningState(true);
                    return false;
                }

                if (DateTime.Now - startWaitTime < TimeSpan.FromMinutes(1))
                {
                    Log.Debug("Waiting for AWS grabber {0} to start.", grabber);
                    Thread.Sleep(5000);
                }

                else
                {
                    Log.Warn("AWS instance did not start after 60 seconds.");
                    return false;
                }

            } while (true);
        }

        public Task<bool> Stop(IInstance grabber, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}