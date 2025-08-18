using Vikcher.Framework.EventBus.Events;
using MediatR;

namespace Patents.ArtRepoCloud.Domain.IntegrationEvents
{
    public class FilesZipIntegrationEvent : IntegrationEvent, INotification
    {
        public FilesZipIntegrationEvent(IEnumerable<string> referenceNumbers)
        {
            ReferenceNumbers = referenceNumbers;
        }

        public IEnumerable<string> ReferenceNumbers { get; }
    }
}