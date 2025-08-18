using Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Vikcher.Framework.Data.Cosmos;

namespace Patents.ArtRepoCloud.Infrastructure.Repositories
{
    public class DocumentRepository : AbstractRepository<DocumentDbContext, ArtRepoDocument>, IDocumentRepository
    {
        private readonly IUnitOfWork<DocumentDbContext> _unitOfWork;

        public DocumentRepository(DocumentDbContext context, IUnitOfWork<DocumentDbContext> unitOfWork) : base(context)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ArtRepoDocument?> GetAsync(string id, CancellationToken cancellationToken)
        {
            return  Context.Documents
                .AsQueryable<ArtRepoDocument>()
                .FirstOrDefault(d => d.Id == id);
        }

        public IQueryable<ArtRepoDocument> QueryDocuments()
        {
            return Context.Documents.AsQueryable<ArtRepoDocument>();
        }

        public async Task<IEnumerable<ArtRepoDocument>> DocumentsAsync(CancellationToken cancellationToken)
        {
            return await Context.Documents
                .AsQueryable<ArtRepoDocument>()
                .ToListAsync(cancellationToken);
        }

        public async Task<ArtRepoDocument?> GetByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken)
        {
            var result = await Context.Documents
                .AsQueryable<ArtRepoDocument>()
                .Where(x => x.ReferenceNumber == referenceNumber)
                .OrderByDescending(d => d.ReferenceNumber)
                .ToListAsync(cancellationToken);

            return result.FirstOrDefault();
        }

        public async Task<IEnumerable<ArtRepoDocument>> GetByReferenceNumbersAsync(IEnumerable<string> referenceNumbers, CancellationToken cancellationToken)
        {
            return await Context.Documents!
                .AsQueryable<ArtRepoDocument>()
                .Where(d => referenceNumbers.Contains(d.ReferenceNumber))
                .OrderByDescending(d => d.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}