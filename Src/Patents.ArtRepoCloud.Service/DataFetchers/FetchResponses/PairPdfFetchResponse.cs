using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.DataFetchers.Contracts;
using Patents.ArtRepoCloud.Service.Enums;

namespace Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses
{
    public class PairPdfFetchResponse : IFetchResponse
    {
        public PairPdfFetchResponse(ReferenceNumber referenceNumber, HttpReasonCode httpReasonCode, ImportSource source)
        {
            ReferenceNumber = referenceNumber;
            PdfFiles = Enumerable.Empty<UsptoFileData>();
            HttpReasonCode = httpReasonCode;
            Source = source;
        }

        public PairPdfFetchResponse(ReferenceNumber referenceNumber, IEnumerable<UsptoFileData> pdfFiles, HttpReasonCode httpReasonCode, ImportSource source)
        {
            ReferenceNumber = referenceNumber;
            PdfFiles = pdfFiles;
            HttpReasonCode = httpReasonCode;
            Source = source;
        }

        public ReferenceNumber ReferenceNumber { get; }
        public IEnumerable<UsptoFileData> PdfFiles { get; }
        public HttpReasonCode HttpReasonCode { get; }
        public ImportSource Source { get; }
    }
}