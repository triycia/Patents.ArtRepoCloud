namespace Patents.ArtRepoCloud.Service.DataFetchers.Contracts
{
    public class FileData
    {
        public FileData(Guid onDiskId, string fileName, string filePath, string mediaType, int pageCount, long length, int sequence = 0)
        {
            OnDiskId = onDiskId;
            FileName = fileName;
            FilePath = filePath;
            MediaType = mediaType;
            PageCount = pageCount;
            Length = length;
            Sequence = sequence;
        }

        public string FilePath { get; }
        public Guid OnDiskId { get; }
        public string FileName { get; }
        public string MediaType { get; }
        public long Length { get; }
        public int Sequence { get; }
        public int PageCount { get; }
    }
}