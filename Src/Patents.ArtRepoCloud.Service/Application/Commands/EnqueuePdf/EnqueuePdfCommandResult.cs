namespace Patents.ArtRepoCloud.Service.Application.Commands.EnqueuePdf
{
    public class EnqueuePdfCommandResult
    {
        public EnqueuePdfCommandResult(bool success)
        {
            Success = success;
        }

        public bool Success { get; }
    }
}