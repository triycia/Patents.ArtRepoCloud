using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.DataFetchers.Contracts;
using MediatR;

namespace Patents.ArtRepoCloud.Service.Application.Commands.SaveArtRepoDocumentPdfs
{
    public class SaveArtRepoDocumentPdfsCommand : IRequest<SaveArtRepoDocumentPdfsCommandResult>
    {
        public SaveArtRepoDocumentPdfsCommand(ReferenceNumber referenceNumber, IEnumerable<FileData> pdfFiles, ImportSource source)
        {
            ReferenceNumber = referenceNumber;
            PdfFiles = pdfFiles;
            Source = source;
        }

        public ReferenceNumber ReferenceNumber { get; }
        public IEnumerable<FileData> PdfFiles { get; }
        public ImportSource Source { get; }
    }
}