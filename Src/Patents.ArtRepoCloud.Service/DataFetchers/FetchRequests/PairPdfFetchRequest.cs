using Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;

namespace Patents.ArtRepoCloud.Service.DataFetchers.FetchRequests
{
    public class PairPdfFetchRequest : IFetchRequest
    {
        public PairPdfFetchRequest(ReferenceNumber referenceNumber, string customerNumber, IEnumerable<PairFileData> documentPairFiles)
        {
            ReferenceNumber = referenceNumber;
            CustomerNumber = customerNumber;
            DocumentPairFiles = documentPairFiles;
        }

        public ReferenceNumber ReferenceNumber { get; }
        public string CustomerNumber { get; }
        public IEnumerable<PairFileData> DocumentPairFiles { get; }
    }
}