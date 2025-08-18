using MediatR;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.DocumentFileImageExtraction
{
    public class DocumentFileImageExtractionCommand : IRequest<DocumentFileImageExtractionCommandResult>
    {
        public DocumentFileImageExtractionCommand(string referenceNumber, int[]? pages = null)
        {
            ReferenceNumber = referenceNumber;
            Pages = pages;
        }

        public string ReferenceNumber { get; }
        public int[]? Pages { get; }
    }
}