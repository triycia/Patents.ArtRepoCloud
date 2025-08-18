using Patents.ArtRepoCloud.Domain.Attributes;
using Patents.ArtRepoCloud.Domain.Configuration;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.IntegrationEvents;
using Patents.ArtRepoCloud.Service.EventBusSettings.Interfaces;

namespace Patents.ArtRepoCloud.Service.EventBusSettings
{
    [QueueClientType(ImportPriority.Normal, ImportSource.Uspto)]
    public class PAIRServiceBusNormalPriorityQueueSettings : IPriorityEventBusQueueSettings
    {
        private readonly ServiceBusSettings _appSettings;

        public PAIRServiceBusNormalPriorityQueueSettings(ServiceBusSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public bool UseManagedIdentity => false;
        public string ConnectionString => _appSettings.ConnectionString;
        public string QueueName => _appSettings.PAIRQueue.NormalPriority;
        public string EventAssemblyName => typeof(PAIRDocumentIntegrationEvent).Assembly.FullName;
        public string EventNamespace => typeof(PAIRDocumentIntegrationEvent).Namespace;
    }
}