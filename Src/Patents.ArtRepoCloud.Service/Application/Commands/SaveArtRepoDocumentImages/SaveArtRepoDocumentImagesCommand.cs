using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.DataFetchers.Contracts;
using MediatR;

namespace Patents.ArtRepoCloud.Service.Application.Commands.SaveArtRepoDocumentImages
{
    public class SaveArtRepoDocumentImagesCommand : IRequest<SaveArtRepoDocumentImagesCommandResult>
    {
        public SaveArtRepoDocumentImagesCommand(ReferenceNumber referenceNumber, IEnumerable<ImageData> files, ImportSource source)
        {
            ReferenceNumber = referenceNumber;
            Files = files;
            Source = source;
        }

        public ReferenceNumber ReferenceNumber { get; }
        public IEnumerable<ImageData> Files { get; }
        public ImportSource Source { get; }
    }
}