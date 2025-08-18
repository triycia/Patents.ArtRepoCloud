namespace Patents.ArtRepoCloud.Service.Application.Queries.ArtRepoDocument
{
    public class ArtRepoDocumentQueryResult
    {
        public ArtRepoDocumentQueryResult(Domain.Aggregates.DocumentAggregate.ArtRepoDocument? document)
        {
            Document = document;
        }

        public Domain.Aggregates.DocumentAggregate.ArtRepoDocument? Document { get; }
    }
}