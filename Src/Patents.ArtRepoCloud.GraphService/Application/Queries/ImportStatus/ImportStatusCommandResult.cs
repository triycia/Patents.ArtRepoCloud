using Patents.ArtRepoCloud.Domain.Enums;

namespace Patents.ArtRepoCloud.GraphService.Application.Queries.ImportStatus
{
    public class ImportStatusCommandResult
    {
        public ImportStatusCommandResult(IEnumerable<ReferenceImportStatus> statuses)
        {
            Statuses = statuses;
        }

        public IEnumerable<ReferenceImportStatus> Statuses { get; }

        public class ReferenceImportStatus
        {
            public ReferenceImportStatus(string referenceNumber, QueueStatus status)
            {
                ReferenceNumber = referenceNumber;
                Status = status;
            }

            public string ReferenceNumber { get; }
            public QueueStatus Status { get; }
        }
    }
}