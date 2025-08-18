using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.DataProviders.IpdIfi.Contracts;

namespace Patents.ArtRepoCloud.Service.DataProviders.IpdIfi.Interfaces
{
    public interface IIpdIfiApiProxy
    {
        Task<IpdIfiDocumentResult?> GetDocumentAsync(ReferenceNumber referenceNumber, CancellationToken cancellationToken);
        Task<IpdIfiImageAttachmentsResult?> GetImageAttachmentsAsync(ReferenceNumber referenceNumber, CancellationToken cancellationToken);
    }
}