using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using MediatR;

namespace Patents.ArtRepoCloud.Service.Application.Commands.EnqueuePdf
{
    public class EnqueuePdfCommand : IRequest<EnqueuePdfCommandResult>
    {
        public EnqueuePdfCommand(
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

        public EnqueuePdfCommand(
            ReferenceNumber referenceNumber,
            ImportPriority priority,
            ImportSource importSource,
            byte retryCount = 0)
        {
            ReferenceNumber = referenceNumber;
            Priority = priority;
            ImportSource = importSource;
            RetryCount = retryCount;
        }

        public ReferenceNumber ReferenceNumber { get; }
        public ImportPriority Priority { get; }
        public ImportSource ImportSource { get; }
        public bool UseSourceStrictly { get; }
        public byte RetryCount { get; }
    }
}