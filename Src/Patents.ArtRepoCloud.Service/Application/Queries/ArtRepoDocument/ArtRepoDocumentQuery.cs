using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using MediatR;

namespace Patents.ArtRepoCloud.Service.Application.Queries.ArtRepoDocument
{
    public class ArtRepoDocumentQuery : IRequest<ArtRepoDocumentQueryResult>
    {
        public ArtRepoDocumentQuery(ReferenceNumber referenceNumber)
        {
            ReferenceNumber = referenceNumber;
        }

        public ReferenceNumber ReferenceNumber { get; }
    }
}