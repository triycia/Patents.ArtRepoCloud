using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.DataFetchers.Contracts;
using Patents.ArtRepoCloud.Service.Enums;

namespace Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses
{
    public class PdfFetchResponse : IFetchResponse
    {
        public PdfFetchResponse(ReferenceNumber referenceNumber, IEnumerable<FileData> pdfFiles, HttpReasonCode httpReasonCode, ImportSource source)
        {
            ReferenceNumber = referenceNumber;
            PdfFiles = pdfFiles;
            HttpReasonCode = httpReasonCode;
            Source = source;
        }

        public PdfFetchResponse(ReferenceNumber referenceNumber, HttpReasonCode httpReasonCode, ImportSource source)
        {
            ReferenceNumber = referenceNumber;
            HttpReasonCode = httpReasonCode;
            Source = source;
            PdfFiles = Enumerable.Empty<FileData>();
        }

        public ReferenceNumber ReferenceNumber { get; }
        public IEnumerable<FileData> PdfFiles { get; }
        public HttpReasonCode HttpReasonCode { get; }
        public ImportSource Source { get; }
    }
}