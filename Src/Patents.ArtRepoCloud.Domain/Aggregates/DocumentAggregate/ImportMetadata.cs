using Patents.ArtRepoCloud.Domain.Enums;
using Newtonsoft.Json;

namespace Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate
{
    public class ImportMetadata
    {
        public ImportMetadata()
        {
            Status = QueueStatus.None;
            ImportDateTime = DateTime.Now;
        }

        [JsonConstructor]
        public ImportMetadata(QueueStatus status, DateTime importDateTime) 
        {
            Status = status;
            ImportDateTime = importDateTime;
        }
        public QueueStatus Status { get; private set; }
        public DateTime ImportDateTime { get; private set; }

        public void SetStatus(QueueStatus status)
        {
            Status = status;
            ImportDateTime = DateTime.Now;
        }
    }
}