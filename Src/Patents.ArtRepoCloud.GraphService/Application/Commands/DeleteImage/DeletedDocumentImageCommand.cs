using MediatR;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.DeleteImage
{
    public class DeletedDocumentImageCommand : IRequest<DeleteImageCommandResult>
    {
        public DeletedDocumentImageCommand(string referenceNumber, Guid imageGuid)
        {
            ReferenceNumber = referenceNumber;
            ImageGuid = imageGuid;
        }

        public string ReferenceNumber { get; }
        public Guid ImageGuid { get; }
    }
}