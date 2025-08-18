namespace Patents.ArtRepoCloud.GraphService.Application.Commands.UpdateDocument
{
    public class UpdateDocumentCommandResult
    {
        public UpdateDocumentCommandResult(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public bool IsSuccess { get; }
    }
}