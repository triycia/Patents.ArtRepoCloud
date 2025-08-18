using MediatR;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.UploadDocumentFile
{
    public class UploadDocumentFileCommand : IRequest<UploadDocumentFileCommandResult>
    {
        public UploadDocumentFileCommand(string referenceNumber, string fileName, int[] designatedImages, int? representativeImagePageNumber, Stream file)
        {
            ReferenceNumber = referenceNumber;
            FileName = fileName;
            DesignatedImages = designatedImages;
            RepresentativeImagePageNumber = representativeImagePageNumber;
            File = file;
        }

        public string ReferenceNumber { get; }
        public string FileName { get; }
        public int[] DesignatedImages { get; }
        public int? RepresentativeImagePageNumber { get; }
        public Stream File { get; }
    }
}