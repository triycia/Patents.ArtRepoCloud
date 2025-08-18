using Patents.ArtRepoCloud.Domain.Enums;
using Newtonsoft.Json;

namespace Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate
{
    public class DocumentFile
    {
        private DocumentFile(){}

        [JsonConstructor]
        public DocumentFile(
            Guid guid, 
            string fileName, 
            string mediaType, 
            string blobPath, 
            int pageCount, 
            long length, 
            DataSource source, 
            DateTime dateCreated)
        {
            Guid = guid;
            FileName = fileName;
            MediaType = mediaType;
            BlobPath = blobPath;
            PageCount = pageCount;
            Length = length;
            Source = source;
            DateCreated = dateCreated;
        }

        public Guid Guid { get; private set; }
        public string FileName { get; private set; }
        public string MediaType { get; private set; }
        public string BlobPath { get; private set; }
        public int PageCount { get; private set; }
        public long Length { get; private set; }
        public DataSource Source { get; private set; }
        public DateTime DateCreated { get; private set; }
    }
}