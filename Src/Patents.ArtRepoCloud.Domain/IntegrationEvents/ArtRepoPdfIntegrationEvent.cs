using Vikcher.Framework.EventBus.Events;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using MediatR;

namespace Patents.ArtRepoCloud.Domain.IntegrationEvents
{
    public class ArtRepoPdfIntegrationEvent : IntegrationEvent, INotification
    {
        public ArtRepoPdfIntegrationEvent(
            ReferenceNumber referenceNumber, 
            ImportPriority priority, 
            ImportSource importSource, 
            byte retryCount, 
            bool useSourceStrictly = false)
        {
            ReferenceNumber = referenceNumber;
            Priority = priority;
            ImportSource = importSource;
            RetryCount = retryCount;
            UseSourceStrictly = useSourceStrictly;
        }

        public ReferenceNumber ReferenceNumber { get; }
        public ImportPriority Priority { get; }
        public ImportSource ImportSource { get; }
        public byte RetryCount { get; }
        public bool UseSourceStrictly { get; }
    }
}