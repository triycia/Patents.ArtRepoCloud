using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using MediatR;

namespace Patents.ArtRepoCloud.Service.Application.Commands.RequeueDocument
{
    public class RequeueDocumentCommand : IRequest<RequeueDocumentCommandResult>
    {
        public RequeueDocumentCommand(
            ReferenceNumber referenceNumber, 
            ImportPriority priority, 
            ImportSource source, 
            bool useSourceStrictly,
            bool isUserImport,
            byte retryCount)
        {
            ReferenceNumber = referenceNumber;
            Priority = priority;
            Source = source;
            UseSourceStrictly = useSourceStrictly;
            IsUserImport = isUserImport;
            RetryCount = retryCount;
        }

        public RequeueDocumentCommand(
            ReferenceNumber referenceNumber, 
            ImportPriority priority, 
            ImportSource source,
            bool isUserImport,
            byte retryCount)
        {
            ReferenceNumber = referenceNumber;
            Priority = priority;
            Source = source;
            IsUserImport = isUserImport;
            RetryCount = retryCount;
        }

        public ReferenceNumber ReferenceNumber { get; }
        public ImportPriority Priority { get; }
        public ImportSource Source { get; }
        public bool UseSourceStrictly { get; }
        public bool IsUserImport { get; }
        public byte RetryCount { get; }
    }
}