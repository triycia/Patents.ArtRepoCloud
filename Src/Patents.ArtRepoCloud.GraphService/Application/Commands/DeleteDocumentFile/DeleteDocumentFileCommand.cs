using MediatR;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.DeleteDocumentFile
{
    public class DeleteDocumentFileCommand : IRequest<DeleteDocumentFileCommandResult>
    {
        public DeleteDocumentFileCommand(string referenceNumber)
        {
            ReferenceNumber = referenceNumber;
        }

        public string ReferenceNumber { get; }
    }
}