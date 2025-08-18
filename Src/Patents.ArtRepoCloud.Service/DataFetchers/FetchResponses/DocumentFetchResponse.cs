using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.Enums;
using Patents.ArtRepoCloud.Service.ValueObjects.BibData;

namespace Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses
{
    public class DocumentFetchResponse : IFetchResponse
    {
        public DocumentFetchResponse(ReferenceNumber referenceNumber, HttpReasonCode httpReasonCode, ImportSource source)
        {
            ReferenceNumber = referenceNumber;
            HttpReasonCode = httpReasonCode;
            Source = source;
        }
        public DocumentFetchResponse(ReferenceNumber referenceNumber, BibDocument? document, HttpReasonCode httpReasonCode, ImportSource source)
        {
            ReferenceNumber = referenceNumber;
            Document = document;
            HttpReasonCode = httpReasonCode;
            Source = source;
        }

        public BibDocument? Document { get; }
        public ReferenceNumber ReferenceNumber { get; }
        public HttpReasonCode HttpReasonCode { get; }
        public ImportSource Source { get; }
    }
}