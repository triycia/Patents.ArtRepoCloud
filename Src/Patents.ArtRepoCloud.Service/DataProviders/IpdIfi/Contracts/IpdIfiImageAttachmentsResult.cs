namespace Patents.ArtRepoCloud.Service.DataProviders.IpdIfi.Contracts
{
    public class IpdIfiImageAttachmentsResult
    {
        public IpdIfiImageAttachmentsResult(List<ImageItem>? images, IpdIfiRequestStatus status)
        {
            Images = images;
            Status = status;
        }

        public List<ImageItem>? Images { get; }
        public IpdIfiRequestStatus Status { get; }

        public class ImageItem
        {
            public ImageItem(string fileName, string? orientation, int sequence)
            {
                FileName = fileName;
                Orientation = string.IsNullOrWhiteSpace(orientation) ? "landscape" : orientation;
                Sequence = sequence;
            }
            public string FileName { get; }
            public string Orientation { get; }
            public int Sequence { get; }
        }
    }
}