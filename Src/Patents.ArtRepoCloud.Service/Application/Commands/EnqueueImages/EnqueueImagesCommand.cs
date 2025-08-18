using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using MediatR;

namespace Patents.ArtRepoCloud.Service.Application.Commands.EnqueueImages
{
    public class EnqueueImagesCommand : IRequest<EnqueueImagesCommandResult>
    {
        public EnqueueImagesCommand(
            ReferenceNumber referenceNumber,
            ImportPriority priority,
            ImportSource importSource,
            bool useSourceStrictly,
            byte retryCount = 0)
        {
            ReferenceNumber = referenceNumber;
            Priority = priority;
            ImportSource = importSource;
            UseSourceStrictly = useSourceStrictly;
            RetryCount = retryCount;
        }

        public ReferenceNumber ReferenceNumber { get; }
        public ImportPriority Priority { get; }
        public ImportSource ImportSource { get; }
        public bool UseSourceStrictly { get; }
        public byte RetryCount { get; }

    }
}