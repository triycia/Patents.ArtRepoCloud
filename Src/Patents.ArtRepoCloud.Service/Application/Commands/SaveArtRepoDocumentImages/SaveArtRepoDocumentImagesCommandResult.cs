namespace Patents.ArtRepoCloud.Service.Application.Commands.SaveArtRepoDocumentImages
{
    public class SaveArtRepoDocumentImagesCommandResult
    {
        public SaveArtRepoDocumentImagesCommandResult(bool success)
        {
            Success = success;
        }

        public bool Success { get; }
    }
}