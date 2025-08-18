using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.ValueObjects;

namespace Patents.ArtRepoCloud.Service.DataFetchers.FetchRequests
{
    public class PairDocumentFetchRequest : IFetchRequest
    {
        public PairDocumentFetchRequest(ReferenceNumber referenceNumber)
        {
            ReferenceNumber = referenceNumber;
        }

        public ReferenceNumber ReferenceNumber { get; }
    }
}