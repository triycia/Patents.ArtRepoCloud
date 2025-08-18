using Vikcher.Framework.Data.Cosmos;
using MediatR;
using Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate;

namespace Patents.ArtRepoCloud.Infrastructure
{
    public class DocumentDbContext : AbstractContext
    {
        public DocumentDbContext(IContextCosmosClientFactory<DocumentDbContext> factory, IMediator mediator) : base(factory, mediator)
        {
        }

        public DocumentContainer<ArtRepoDocument> Documents { get; } = new ("Documents");
    }
}