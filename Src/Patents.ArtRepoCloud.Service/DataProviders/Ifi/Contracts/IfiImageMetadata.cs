namespace Patents.ArtRepoCloud.Service.DataProviders.Ifi.Contracts
{
    public class IfiImageMetadata
    {
        public IfiImageMetadata(string fileName, string ifiPath, string mediaType, string orientation, int sequence)
        {
            FileName = fileName;
            IfiPath = ifiPath;
            MediaType = mediaType;
            Orientation = orientation;
            Sequence = sequence;
        }
        public string FileName { get; }
        public string IfiPath { get; }
        public string MediaType { get; }
        public string Orientation { get; }
        public int Sequence { get; }
    }
}