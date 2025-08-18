namespace Patents.ArtRepoCloud.Service.Application.Commands.EnqueueImages
{
    public class EnqueueImagesCommandResult
    {
        public EnqueueImagesCommandResult(bool success)
        {
            Success = success;
        }

        public bool Success { get; }
    }
}