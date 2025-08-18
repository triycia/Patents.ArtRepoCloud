namespace Patents.ArtRepoCloud.Service.Application.Commands.SavePairDocumentPdfs
{
    public class SavePairDocumentPdfsCommandResult
    {
        public SavePairDocumentPdfsCommandResult(bool success)
        {
            Success = success;
        }

        public bool Success { get; }
    }
}