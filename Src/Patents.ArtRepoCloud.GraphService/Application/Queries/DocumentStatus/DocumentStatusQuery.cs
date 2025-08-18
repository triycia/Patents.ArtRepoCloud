using MediatR;

namespace Patents.ArtRepoCloud.GraphService.Application.Queries.DocumentStatus
{
    public class DocumentStatusQuery : IRequest<DocumentStatusQueryResult>
    {
        public DocumentStatusQuery(string referenceNumber)
        {
            ReferenceNumber = referenceNumber;
        }

        public string ReferenceNumber { get; }
    }
}