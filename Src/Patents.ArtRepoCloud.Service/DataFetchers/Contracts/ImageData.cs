namespace Patents.ArtRepoCloud.Service.DataFetchers.Contracts
{
    public class ImageData
    {
        public ImageData(
            Guid onDiskId, 
            string fileName, 
            string filePath, 
            string mediaType, 
            long length, 
            int sequence = 0)
        {
            OnDiskId = onDiskId;
            FileName = fileName;
            FilePath = filePath;
            MediaType = mediaType;
            Length = length;
            Sequence = sequence;
        }

        public string FilePath { get; }
        public Guid OnDiskId { get; }
        public string FileName { get; }
        public string MediaType { get; }
        public long Length { get; }
        public int Sequence { get; }
    }
}