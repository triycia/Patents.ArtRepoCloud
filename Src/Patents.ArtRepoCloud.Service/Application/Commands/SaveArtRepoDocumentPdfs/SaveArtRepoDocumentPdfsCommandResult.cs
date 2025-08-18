namespace Patents.ArtRepoCloud.Service.Application.Commands.SaveArtRepoDocumentPdfs
{
    public class SaveArtRepoDocumentPdfsCommandResult
    {
        public SaveArtRepoDocumentPdfsCommandResult(bool success)
        {
            Success = success;
        }

        public bool Success { get; }
    }
}