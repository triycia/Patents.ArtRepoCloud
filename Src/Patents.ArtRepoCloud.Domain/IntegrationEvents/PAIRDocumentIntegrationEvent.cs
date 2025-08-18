using Vikcher.Framework.EventBus.Events;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using MediatR;

namespace Patents.ArtRepoCloud.Domain.IntegrationEvents
{
    public class PAIRDocumentIntegrationEvent : IntegrationEvent, INotification
    {
        public PAIRDocumentIntegrationEvent(
            ReferenceNumber referenceNumber, 
            ImportPriority priority,
            byte retryCount = 0)
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