namespace Patents.ArtRepoCloud.Service.DataFetchers.Ifi.Contracts.Attachment
{
    public class ImageFileData
    {
        public ImageFileData(byte[] bytes, string fileName, string mediaType, string orientation, int sequence = 0)
        {
            Bytes = bytes;
            FileName = fileName;
            MediaType = mediaType;
            Orientation = orientation;
            Sequence = sequence;
        }


        public byte[] Bytes { get; }
        public string FileName { get; }
        public string MediaType { get; }
        public string Orientation { get; }
        public int Sequence { get; }
    }
}
