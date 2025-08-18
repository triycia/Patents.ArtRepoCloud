using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.GraphService.DataModels;
using MediatR;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.CreateDocument
{
    public class CreateDocumentCommand : IRequest<CreateDocumentCommandResult>
    {
        public CreateDocumentCommand(
            string referenceNumber,
            string? sourceReferenceNumber,
            string? applicationNumber,
            DocumentType documentType,
            string? country,
            int? termAdjustment,
            string? earliestPriorityNumber,
            DateTime? earliestPriorityDate,
            DateTime? applicationDate,
            DateTime? publicationDate,
            DateTime? dateFiled,
            DateTime? issueDate,
            DateTime? expirationDate,
            DateTime? originalExpirationDate,
            DateTime? adjustedExpirationDate,
            DateTime? pctFilingDate,
            string? assigneeName,
            string? authors,
            string? terminalDisclaimer,
            string? notes,
            string? userUrl,
            DocumentFileModel? file,
            string? temporaryFilesPath,
            int[] designatedImages,
            IEnumerable<ClassificationModel>? documentClassifications,
            IEnumerable<TranslationModel>? documentTranslations)
        {
            ReferenceNumber = referenceNumber;
            SourceReferenceNumber = sourceReferenceNumber;
            ApplicationNumber = applicationNumber;
            DocumentType = documentType;
            Country = country;
            TermAdjustment = termAdjustment;
            EarliestPriorityNumber = earliestPriorityNumber;
            EarliestPriorityDate = earliestPriorityDate;
            ApplicationDate = applicationDate;
            PublicationDate = publicationDate;
            DateFiled = dateFiled;
            IssueDate = issueDate;
            ExpirationDate = expirationDate;
            OriginalExpirationDate = originalExpirationDate;
            AdjustedExpirationDate = adjustedExpirationDate;
            PctFilingDate = pctFilingDate;
            AssigneeName = assigneeName;
            Authors = authors;
            TerminalDisclaimer = terminalDisclaimer;
            Notes = notes;
            UserUrl = userUrl;
            File = file;
            TemporaryFilesPath = temporaryFilesPath;
            DesignatedImages = designatedImages;
            DocumentClassifications = documentClassifications;
            DocumentTranslations = documentTranslations ?? new List<TranslationModel>();
        }

        public string ReferenceNumber { get; private set; }
        public string? SourceReferenceNumber { get; private set; }
        public string? ApplicationNumber { get; private set; }
        public DocumentType DocumentType { get; private set; }
        public string? Country { get; }
        public int? TermAdjustment { get; }
        public string? EarliestPriorityNumber { get; }
        public DateTime? EarliestPriorityDate { get; }
        public DateTime? ApplicationDate { get; }
        public DateTime? PublicationDate { get; }
        public DateTime? DateFiled { get; }
        public DateTime? IssueDate { get; }
        public DateTime? ExpirationDate { get; }
        public DateTime? OriginalExpirationDate { get; }
        public DateTime? AdjustedExpirationDate { get; }
        public DateTime? PctFilingDate { get; }
        public string? AssigneeName { get; }
        public string? Authors { get; }
        public string? TerminalDisclaimer { get; }
        public string? Notes { get; }
        public string? UserUrl { get; }
        public DocumentFileModel? File { get; }
        public string? TemporaryFilesPath { get; }
        public int[] DesignatedImages { get; }

        public IEnumerable<ClassificationModel>? DocumentClassifications { get; private set; }
        public IEnumerable<TranslationModel>? DocumentTranslations { get; }
    }
}