using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.DataFetchers.Contracts;
using MediatR;

namespace Patents.ArtRepoCloud.Service.Application.Commands.SavePairDocumentPdfs
{
    public class SavePairDocumentPdfsCommand : IRequest<SavePairDocumentPdfsCommandResult>
    {
        public SavePairDocumentPdfsCommand(ReferenceNumber referenceNumber, IEnumerable<UsptoFileData> pdfFiles)
        {
            ReferenceNumber = referenceNumber;
            PdfFiles = pdfFiles;
        }

        public ReferenceNumber ReferenceNumber { get; }
        public IEnumerable<UsptoFileData> PdfFiles { get; }
    }
}