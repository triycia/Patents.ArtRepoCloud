using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.Enums;

namespace Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses
{
    public interface IFetchResponse
    {
        public ReferenceNumber ReferenceNumber { get; }
        public HttpReasonCode HttpReasonCode { get; }
        public ImportSource Source { get; }
    }
}