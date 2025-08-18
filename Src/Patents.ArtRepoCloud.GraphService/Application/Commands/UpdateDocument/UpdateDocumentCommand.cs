using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.GraphService.DataModels;
using MediatR;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.UpdateDocument
{
    public class UpdateDocumentCommand : IRequest<UpdateDocumentCommandResult> 
    {
        public UpdateDocumentCommand(
            string referenceNumber,
            DocumentType documentType,
            DateTime? publicationDate,
            DateTime? expirationDate,
            string? assigneeName,
            string? temporaryFilesPath,
            int[] designatedImages,
            TranslationModel documentTranslation, DocumentFileModel? file)
        {
            ReferenceNumber = referenceNumber;
            DocumentType = documentType;
            PublicationDate = publicationDate;
            ExpirationDate = expirationDate;
            AssigneeName = assigneeName;
            TemporaryFilesPath = temporaryFilesPath;
            DesignatedImages = designatedImages;
            DocumentTranslation = documentTranslation;
            File = file;
        }

        public string ReferenceNumber { get; }
        public DocumentType DocumentType { get; }
        public DateTime? PublicationDate { get; }
        public DateTime? ExpirationDate { get; }
        public string? AssigneeName { get; }
        public string? TemporaryFilesPath { get; }
        public int[] DesignatedImages { get; }
        public TranslationModel DocumentTranslation { get; }
        public DocumentFileModel? File { get; }
    }
}