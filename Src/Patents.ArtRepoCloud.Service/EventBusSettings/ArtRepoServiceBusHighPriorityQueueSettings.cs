using Patents.ArtRepoCloud.Domain.Attributes;
using Patents.ArtRepoCloud.Domain.Configuration;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.IntegrationEvents;
using Patents.ArtRepoCloud.Service.EventBusSettings.Interfaces;

namespace Patents.ArtRepoCloud.Service.EventBusSettings
{
    [QueueClientType(ImportPriority.High, ImportSource.All)]
    internal class ArtRepoServiceBusHighPriorityQueueSettings : IPriorityEventBusQueueSettings
    {
        private readonly ServiceBusSettings _appSettings;

        public ArtRepoServiceBusHighPriorityQueueSettings(ServiceBusSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public bool UseManagedIdentity => false;
        public string ConnectionString => _appSettings.ConnectionString;
        public string QueueName => _appSettings.ArtRepoQueue.HighPriority;
        public string EventAssemblyName => typeof(ArtRepoDocumentIntegrationEvent).Assembly.FullName;
        public string EventNamespace => typeof(ArtRepoDocumentIntegrationEvent).Namespace;
    }
}