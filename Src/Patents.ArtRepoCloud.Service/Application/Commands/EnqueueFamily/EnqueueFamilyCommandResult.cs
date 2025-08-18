namespace Patents.ArtRepoCloud.Service.Application.Commands.EnqueueFamily
{
    public class EnqueueFamilyCommandResult
    {
        public EnqueueFamilyCommandResult(bool success)
        {
            Success = success;
        }

        public bool Success { get; }
    }
}