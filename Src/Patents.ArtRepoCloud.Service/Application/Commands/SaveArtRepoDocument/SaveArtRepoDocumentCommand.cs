using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.ValueObjects.BibData;
using MediatR;

namespace Patents.ArtRepoCloud.Service.Application.Commands.SaveArtRepoDocument
{
    public class SaveArtRepoDocumentCommand : IRequest<SaveArtRepoDocumentCommandResult>
    {
        public SaveArtRepoDocumentCommand(ReferenceNumber referenceNumber, BibDocument document, DataSource dataSource, bool isUserImport = false)
        {
            ReferenceNumber = referenceNumber;
            Document = document;
            DataSource = dataSource;
            IsUserImport = isUserImport;
        }

        public ReferenceNumber ReferenceNumber { get; }
        public BibDocument Document { get; }
        public DataSource DataSource { get; }
        public bool IsUserImport { get; }
    }
}