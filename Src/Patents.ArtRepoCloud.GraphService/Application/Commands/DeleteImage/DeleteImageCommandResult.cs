namespace Patents.ArtRepoCloud.GraphService.Application.Commands.DeleteImage
{
    public class DeleteImageCommandResult
    {
        public DeleteImageCommandResult(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public bool IsSuccess { get; }
    }
}