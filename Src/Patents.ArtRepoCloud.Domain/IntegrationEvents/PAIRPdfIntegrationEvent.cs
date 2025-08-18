using Vikcher.Framework.EventBus.Events;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using MediatR;

namespace Patents.ArtRepoCloud.Domain.IntegrationEvents
{
    public class PAIRPdfIntegrationEvent : IntegrationEvent, INotification
    {
        public PAIRPdfIntegrationEvent(ReferenceNumber referenceNumber, ImportPriority priority, byte retryCount)
        {
            ReferenceNumber = referenceNumber;
            Priority = priority;
            RetryCount = retryCount;
        }

        public ReferenceNumber ReferenceNumber { get; }
        public ImportPriority Priority { get; }
        public byte RetryCount { get; }
    }
}