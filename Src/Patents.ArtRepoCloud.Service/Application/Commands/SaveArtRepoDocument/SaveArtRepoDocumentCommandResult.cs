using Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate;

namespace Patents.ArtRepoCloud.Service.Application.Commands.SaveArtRepoDocument
{
    public class SaveArtRepoDocumentCommandResult
    {
        public SaveArtRepoDocumentCommandResult(ArtRepoDocument document)
        {
            Document = document;
        }

        public ArtRepoDocument Document { get; }
    }
}