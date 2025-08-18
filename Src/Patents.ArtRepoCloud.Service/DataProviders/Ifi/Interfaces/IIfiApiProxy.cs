using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.DataProviders.Ifi.Contracts;

namespace Patents.ArtRepoCloud.Service.DataProviders.Ifi.Interfaces
{
    public interface IIfiApiProxy
    {
        Task<string> GetDocumentAsync(string ucid, CancellationToken cancellationToken);
        Task<string?> GetUcidAsync(IReferenceNumber referenceNumber, CancellationToken cancellationToken);
        Task<IList<IfiAttachment>> AttachmentListAsync(string ucid, CancellationToken cancellationToken);
        Task<Stream> AttachmentFetchAsync(string ifiPath, CancellationToken cancellationToken);
        Task<Stream> AttachmentFetchAllAsync(string ucid, CancellationToken cancellationToken);
        Task<IList<string>> GetFamilySimpleAsync(int familyId, CancellationToken cancellationToken);
        Task<IList<string>> GetFamilyExtendedAsync(string ucid, CancellationToken cancellationToken);
    }
}