namespace Patents.ArtRepoCloud.GraphService.Application.Commands.ImageExtraction
{
    public class ImageExtractionCommandResult
    {
        public ImageExtractionCommandResult(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public bool IsSuccess { get; }
    }
}