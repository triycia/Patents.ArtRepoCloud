namespace Patents.ArtRepoCloud.Service.Application.Commands.RequeueDocument
{
    public class RequeueDocumentCommandResult
    {
        public RequeueDocumentCommandResult(bool success, byte retryCount)
        {
            Success = success;
            RetryCount = retryCount;
        }
        public RequeueDocumentCommandResult(bool success)
        {
            Success = success;
        }

        public bool Success { get; }
        public byte? RetryCount { get; }
    }
}