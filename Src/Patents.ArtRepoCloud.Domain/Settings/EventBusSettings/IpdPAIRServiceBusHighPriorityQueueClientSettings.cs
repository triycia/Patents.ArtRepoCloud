using Vikcher.Framework.EventBus.Abstractions.Queue;
using Patents.ArtRepoCloud.Domain.Attributes;
using Patents.ArtRepoCloud.Domain.Configuration;
using Patents.ArtRepoCloud.Domain.Enums;

namespace Patents.ArtRepoCloud.Domain.Settings.EventBusSettings
{
    [QueueClientType(ImportPriority.High, ImportSource.Uspto)]
    public class IpdPAIRServiceBusHighPriorityQueueClientSettings : IEventBusQueueClientSettings
    {
        private readonly ServiceBusSettings _serviceBusSettings;

        public IpdPAIRServiceBusHighPriorityQueueClientSettings(ServiceBusSettings serviceBusSettings)
        {
            _serviceBusSettings = serviceBusSettings;
        }

        public bool UseManagedIdentity => false;
        public string ConnectionString => _serviceBusSettings.ConnectionString;
        public string QueueName => _serviceBusSettings.PAIRQueue.HighPriority;
    }
}