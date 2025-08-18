using Patents.ArtRepoCloud.Domain.ReferenceNumbers;

namespace Patents.ArtRepoCloud.Service.DataFetchers.FetchRequests
{
    public class DocumentFetchRequest : IFetchRequest
    {
        public DocumentFetchRequest(ReferenceNumber referenceNumber)
        {
            ReferenceNumber = referenceNumber;
        }

        public ReferenceNumber ReferenceNumber { get; }
    }
}