using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using MediatR;

namespace Patents.ArtRepoCloud.Service.Application.Commands.ImportStatus
{
    public class ImportStatusCommand : IRequest<ImportStatusCommandResult>
    {
        public ImportStatusCommand(IReferenceNumber referenceNumber, QueueStatus status)
        {
            ReferenceNumber = referenceNumber;
            Status = status;
        }

        public IReferenceNumber ReferenceNumber { get; }
        public QueueStatus Status { get; }
    }
}