using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.DataFetchers;

namespace Patents.ArtRepoCloud.Service.DataProviders.Epo.Interfaces
{
    public interface IEpoApiProxy
    {
        Task<EpoXDocument> GetBibDocumentAsync(ReferenceNumber referenceNumber, CancellationToken cancellationToken);
        Task<string> GetDescriptionAsync(ReferenceNumber referenceNumber, CancellationToken cancellationToken);
        Task<string> GetClaimsAsync(ReferenceNumber referenceNumber, CancellationToken cancellationToken);
        Task<ImageXMetadata> GetImageMetadataAsync(ReferenceNumber referenceNumber, CancellationToken cancellationToken);
        Task<Stream> GetAttachmentAsync(string link, int pageNumber, string mediaType, CancellationToken cancellationToken);
    }
}