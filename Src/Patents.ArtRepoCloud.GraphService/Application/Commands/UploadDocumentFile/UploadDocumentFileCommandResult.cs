namespace Patents.ArtRepoCloud.GraphService.Application.Commands.UploadDocumentFile
{
    public class UploadDocumentFileCommandResult
    {
        public UploadDocumentFileCommandResult(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public bool IsSuccess { get; }
    }
}