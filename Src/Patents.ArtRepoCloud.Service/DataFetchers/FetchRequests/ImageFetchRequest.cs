using Patents.ArtRepoCloud.Domain.ReferenceNumbers;

namespace Patents.ArtRepoCloud.Service.DataFetchers.FetchRequests
{
    public class ImageFetchRequest : IFetchRequest
    {
        public ImageFetchRequest(ReferenceNumber referenceNumber)
        {
            ReferenceNumber = referenceNumber;
        }

        public ReferenceNumber ReferenceNumber { get; }
    }
}