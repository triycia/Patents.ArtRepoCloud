using Patents.ArtRepoCloud.Domain.Configuration;
using Patents.ArtRepoCloud.Domain.IntegrationEvents;
using Vikcher.Framework.EventBus.Abstractions.Queue;

namespace Patents.ArtRepoCloud.Service.EventBusSettings
{
    internal class ArtRepoFileServiceBusQueueSettings : IEventBusQueueSettings
    {
        private readonly ServiceBusSettings _appSettings;

        public ArtRepoFileServiceBusQueueSettings(ServiceBusSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public bool UseManagedIdentity => false;
        public string ConnectionString => _appSettings.ConnectionString;
        public string QueueName => _appSettings.ArtRepoFileQueue;
        public string EventAssemblyName => typeof(ArtRepoDocumentIntegrationEvent).Assembly.FullName;
        public string EventNamespace => typeof(ArtRepoDocumentIntegrationEvent).Namespace;
    }
}