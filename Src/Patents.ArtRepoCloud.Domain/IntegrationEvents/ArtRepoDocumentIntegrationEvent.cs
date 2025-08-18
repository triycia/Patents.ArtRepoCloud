using Vikcher.Framework.EventBus.Events;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using MediatR;

namespace Patents.ArtRepoCloud.Domain.IntegrationEvents
{
    public class ArtRepoDocumentIntegrationEvent : IntegrationEvent, INotification
    {
        public ArtRepoDocumentIntegrationEvent(
            ReferenceNumber referenceNumber,
            ImportPriority priority,
            ImportSource importSource,
            bool useSourceStrictly,
            bool isUserImport = false,
            byte retryCount = 0)
        {
            ReferenceNumber = referenceNumber;
            Priority = priority;
            ImportSource = importSource;
            UseSourceStrictly = useSourceStrictly;
            IsUserImport = isUserImport;
            RetryCount = retryCount;
        }

        public ReferenceNumber ReferenceNumber { get; }
        public ImportPriority Priority { get; }
        public ImportSource ImportSource { get; }
        public bool UseSourceStrictly { get; }
        public bool IsUserImport { get; }
        public byte RetryCount { get; }
    }
}