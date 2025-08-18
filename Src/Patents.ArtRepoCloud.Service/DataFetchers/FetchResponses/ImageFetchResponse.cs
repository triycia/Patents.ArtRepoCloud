using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.DataFetchers.Contracts;
using Patents.ArtRepoCloud.Service.Enums;

namespace Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses
{
    public class ImageFetchResponse : IFetchResponse
    {
        public ImageFetchResponse(ReferenceNumber referenceNumber, HttpReasonCode httpReasonCode, ImportSource source)
        {
            ReferenceNumber = referenceNumber;
            HttpReasonCode = httpReasonCode;
            Source = source;
            Files = Enumerable.Empty<ImageData>();
        }
        public ImageFetchResponse(ReferenceNumber referenceNumber, List<ImageData> files, HttpReasonCode httpReasonCode, ImportSource source)
        {
            ReferenceNumber = referenceNumber;
            Files = files;
            HttpReasonCode = httpReasonCode;
            Source = source;
        }

        public IEnumerable<ImageData> Files { get; }
        public ReferenceNumber ReferenceNumber { get; }
        public HttpReasonCode HttpReasonCode { get; }
        public ImportSource Source { get; }
    }
}