using Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate;
using Vikcher.Framework.Data.Cosmos;

namespace Patents.ArtRepoCloud.Domain.Interfaces
{
    public interface IDocumentRepository : IRepository<ArtRepoDocument>
    {
        Task<ArtRepoDocument?> GetAsync(string id, CancellationToken cancellationToken);

        Task<ArtRepoDocument?> GetByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken);

        Task<IEnumerable<ArtRepoDocument>> GetByReferenceNumbersAsync(IEnumerable<string> referenceNumbers, CancellationToken cancellationToken);

        IQueryable<ArtRepoDocument> QueryDocuments();
        Task<IEnumerable<ArtRepoDocument>> DocumentsAsync(CancellationToken cancellationToken);

        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}