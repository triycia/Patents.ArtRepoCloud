using Vikcher.Framework.EventBus.Abstractions.Queue;
using Patents.ArtRepoCloud.Domain.Attributes;
using Patents.ArtRepoCloud.Domain.Configuration;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.IntegrationEvents;
using Patents.ArtRepoCloud.Service.EventBusSettings.Interfaces;

namespace Patents.ArtRepoCloud.Service.EventBusSettings
{
    [QueueClientType(ImportPriority.Idle, ImportSource.Uspto)]
    internal class PAIRServiceBusIdlePriorityQueueSettings : IPriorityEventBusQueueSettings
    {
        private readonly ServiceBusSettings _appSettings;

        public PAIRServiceBusIdlePriorityQueueSettings(ServiceBusSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public bool UseManagedIdentity => false;
        public string ConnectionString => _appSettings.ConnectionString;
        public string QueueName => _appSettings.PAIRQueue.IdlePriority;
        public string EventAssemblyName => typeof(PAIRDocumentIntegrationEvent).Assembly.FullName;
        public string EventNamespace => typeof(PAIRDocumentIntegrationEvent).Namespace;
    }
}