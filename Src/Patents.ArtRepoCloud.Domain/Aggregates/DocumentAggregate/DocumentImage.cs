using Patents.ArtRepoCloud.Domain.Enums;
using Newtonsoft.Json;

namespace Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate
{
    public class DocumentImage
    {
        private DocumentImage(){}

        [JsonConstructor]
        public DocumentImage(
            Guid guid,
            string fileName,
            string mediaType,
            string blobPath,
            long length,
            DataSource source,
            DateTime dateCreated,
            int sequence,
            int? pageNumber,
            bool isRepresentative = false)
        {
            Guid = guid;
            FileName = fileName;
            MediaType = mediaType;
            BlobPath = blobPath;
            Length = length;
            Source = source;
            DateCreated = dateCreated;
            Sequence = sequence;
            PageNumber = pageNumber;
            IsRepresentative = isRepresentative;
        }

        public DocumentImage(
            Guid guid,
            string fileName,
            string mediaType,
            string blobPath,
            long length,
            DataSource source,
            DateTime dateCreated,
            int sequence, 
            bool isRepresentative = false)
        {
            Guid = guid;
            FileName = fileName;
            MediaType = mediaType;
            BlobPath = blobPath;
            Length = length;
            Source = source;
            DateCreated = dateCreated;
            Sequence = sequence;
            IsRepresentative = isRepresentative;
        }

        public Guid Guid { get; private set; }
        public string FileName { get; private set; }
        public string MediaType { get; private set; }
        public long Length { get; private set; }
        public string BlobPath { get; private set; }
        public DataSource Source { get; private set; }
        public DateTime DateCreated { get; private set; }
        public int Sequence { get; private set; }
        public bool IsRepresentative { get; private set; }
        public int? PageNumber { get; private set; }
    }
}