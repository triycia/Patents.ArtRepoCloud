using Patents.ArtRepoCloud.Domain.Configuration;
using Vikcher.Framework.EventBus.Abstractions.Queue;

namespace Patents.ArtRepoCloud.Domain.Settings.EventBusSettings
{
    public class ArtRepoFileServiceBusQueueClientSettings : IEventBusQueueClientSettings
    {
        private readonly ServiceBusSettings _serviceBusSettings;

        public ArtRepoFileServiceBusQueueClientSettings(ServiceBusSettings serviceBusSettings)
        {
            _serviceBusSettings = serviceBusSettings;
        }

        public bool UseManagedIdentity => false;
        public string ConnectionString => _serviceBusSettings.ConnectionString;
        public string QueueName => _serviceBusSettings.ArtRepoFileQueue;
    }
}