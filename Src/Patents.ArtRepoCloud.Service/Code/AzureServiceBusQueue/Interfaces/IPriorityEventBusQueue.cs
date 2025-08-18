using Patents.ArtRepoCloud.Service.EventBusSettings.Interfaces;
using Vikcher.Framework.EventBus.Abstractions.Queue;

namespace Patents.ArtRepoCloud.Service.Code.AzureServiceBusQueue.Interfaces
{
    public interface IPriorityEventBusQueue<TEventBusQueueSettings> : IEventBusQueue, IDisposable where TEventBusQueueSettings : IPriorityEventBusQueueSettings
    {
    }
}