namespace Patents.ArtRepoCloud.GraphService.DataModels
{
    public class DocumentFileModel
    {
        public DocumentFileModel(
            string fileName,
            string mediaType,
            Guid onDiskGuid,
            string blobPath)
        {
            FileName = fileName;
            MediaType = mediaType;
            OnDiskGuid = onDiskGuid;
            BlobPath = blobPath;
        }

        public Guid OnDiskGuid { get; }
        public string FileName { get; }
        public string MediaType { get; }
        public string BlobPath { get; }
    }
}