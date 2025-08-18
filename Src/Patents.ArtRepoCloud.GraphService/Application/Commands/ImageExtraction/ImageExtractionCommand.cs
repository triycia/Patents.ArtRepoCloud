using MediatR;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.ImageExtraction
{
    public class ImageExtractionCommand : IRequest<ImageExtractionCommandResult>
    {
        public ImageExtractionCommand(Guid onDiskGuid, string relativeBlobPath, int[]? pages = null)
        {
            OnDiskGuid = onDiskGuid;
            RelativeBlobPath = relativeBlobPath;
            Pages = pages;
        }

        public Guid OnDiskGuid { get; }
        public string RelativeBlobPath { get; }
        public int[]? Pages { get; }
    }
}