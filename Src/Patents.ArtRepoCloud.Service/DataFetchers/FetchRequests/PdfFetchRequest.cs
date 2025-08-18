using Patents.ArtRepoCloud.Domain.ReferenceNumbers;

namespace Patents.ArtRepoCloud.Service.DataFetchers.FetchRequests
{
    public class PdfFetchRequest: IFetchRequest
    {
        public PdfFetchRequest(ReferenceNumber referenceNumber)
        {
            ReferenceNumber = referenceNumber;
        }

        public ReferenceNumber ReferenceNumber { get; }
    }
}