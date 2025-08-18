namespace Patents.ArtRepoCloud.GraphService.Application.Commands.DeleteDocumentFile
{
    public class DeleteDocumentFileCommandResult
    {
        public DeleteDocumentFileCommandResult(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public bool IsSuccess { get; }
    }
}