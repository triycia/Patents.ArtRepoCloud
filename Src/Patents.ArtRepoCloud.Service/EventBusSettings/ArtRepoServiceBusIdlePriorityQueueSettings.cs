using Patents.ArtRepoCloud.Domain.Attributes;
using Patents.ArtRepoCloud.Domain.Configuration;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.IntegrationEvents;
using Patents.ArtRepoCloud.Service.EventBusSettings.Interfaces;

namespace Patents.ArtRepoCloud.Service.EventBusSettings
{
    [QueueClientType(ImportPriority.Idle, ImportSource.All)]
    internal class ArtRepoServiceBusIdlePriorityQueueSettings : IPriorityEventBusQueueSettings
    {
        private readonly ServiceBusSettings _appSettings;

        public ArtRepoServiceBusIdlePriorityQueueSettings(ServiceBusSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public bool UseManagedIdentity => false;
        public string ConnectionString => _appSettings.ConnectionString;
        public string QueueName => _appSettings.ArtRepoQueue.IdlePriority;
        public string EventAssemblyName => typeof(ArtRepoDocumentIntegrationEvent).Assembly.FullName;
        public string EventNamespace => typeof(ArtRepoDocumentIntegrationEvent).Namespace;
    }
}