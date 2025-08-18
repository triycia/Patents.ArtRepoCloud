namespace Patents.ArtRepoCloud.Service.Application.Commands.ImportStatus
{
    public class ImportStatusCommandResult
    {
        public ImportStatusCommandResult(bool success)
        {
            Success = success;
        }

        public bool Success { get; }
    }
}