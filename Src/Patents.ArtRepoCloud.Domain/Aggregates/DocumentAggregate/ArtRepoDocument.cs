using System.ComponentModel.DataAnnotations.Schema;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.Extensions;
using Vikcher.Framework.Data.Cosmos;
using Microsoft.Azure.Cosmos;
using HotChocolate;
using Newtonsoft.Json;

namespace Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate
{
    public class ArtRepoDocument : Document, IAggregateRoot
    {
        public ArtRepoDocument() { }

        public ArtRepoDocument(string referenceNumber)
        {
            Id = Guid.NewGuid().ToString();
            ReferenceNumber = referenceNumber;

            ImportStatus = new ImportMetadata();

            SetPartitionKey();
        }

        [JsonConstructor]
        public ArtRepoDocument(
            string id,
            string referenceNumber,
            string applicationNumber,
            string? publicationNumber,
            string? patentNumber,
            string? pctNumber,
            DocumentType documentType, 
            int? patentFamilyId,
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
            string? documentDataBlobPath,
            IList<string> familySimples,
            IList<string> familyExtendeds,
            IList<DocumentImage> documentImages,
            DocumentMetadata documentMetadata,
            ImportMetadata? importStatus,
            DocumentFile? documentFile,
            PairData? pairData)
        {
            Id = id;
            ReferenceNumber = referenceNumber;
            ApplicationNumber = applicationNumber;
            PublicationNumber = publicationNumber;
            PatentNumber = patentNumber;
            PctNumber = pctNumber;
            DocumentType = documentType;
            PatentFamilyId = patentFamilyId;
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
            DocumentDataBlobPath = documentDataBlobPath;
            FamilySimples = familySimples;
            FamilyExtendeds = familyExtendeds;
            DocumentImages = documentImages;
            DocumentMetadata = documentMetadata;
            ImportStatus = importStatus;
            DocumentFile = documentFile;
            PairData = pairData;

            SetPartitionKey();
        }

        public ArtRepoDocument(
            string referenceNumber,
            string applicationNumber,
            string? publicationNumber,
            string? patentNumber,
            string? pctNumber,
            DocumentType documentType, 
            int? patentFamilyId,
            string country,
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
            DataSource documentDataSource,
            string? dataSourceReferenceNumber,
            DateTime dateCreated)
        {
            Id = Guid.NewGuid().ToString();
            ReferenceNumber = referenceNumber;
            ApplicationNumber = applicationNumber;
            PublicationNumber = publicationNumber;
            PatentNumber = patentNumber;
            PctNumber = pctNumber;
            DocumentType = documentType;
            PatentFamilyId = patentFamilyId;
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

            DocumentMetadata = new DocumentMetadata(documentDataSource, dataSourceReferenceNumber, dateCreated);

            SetPartitionKey();
        }

        public string ReferenceNumber { get; private set; }
        public string? ApplicationNumber { get; private set; }
        public string? PublicationNumber { get; private set; }
        public string? PatentNumber { get; private set; }
        public string? PctNumber { get; private set; }
        public string? EarliestPriorityNumber { get; private set; }
        public string? Country { get; private set; }
        public DocumentType DocumentType { get; private set; }
        public int? PatentFamilyId { get; private set; }
        public int? TermAdjustment { get; private set; }
        public DateTime? EarliestPriorityDate { get; private set; }
        public DateTime? ApplicationDate { get; private set; }
        public DateTime? PublicationDate { get; private set; }
        public DateTime? DateFiled { get; private set; }
        public DateTime? IssueDate { get; private set; }
        public DateTime? ExpirationDate { get; private set; }
        public DateTime? OriginalExpirationDate { get; private set; }
        public DateTime? AdjustedExpirationDate { get; private set; }
        public DateTime? PctFilingDate { get; private set; }

        public IList<string> FamilySimples { get; private set; } = new List<string>();
        public IList<string> FamilyExtendeds { get; private set; } = new List<string>();
        public IList<DocumentImage> DocumentImages { get; private set; } = new List<DocumentImage>();

        public ImportMetadata? ImportStatus { get; private set; }
        public DocumentMetadata DocumentMetadata { get; private set; } = new();
        public DocumentFile? DocumentFile { get; private set; }
        public PairData? PairData { get; private set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public DocumentData? DocumentData { get; private set; }

        [GraphQLIgnore]
        public string? DocumentDataBlobPath { get; private set; }

        #region GraphAutoProperties

        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public DataSource DataSource => DocumentMetadata.DocumentDataSource;

        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public DateTime DateModified => DocumentMetadata.DateModified;

        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public DateTime DateCreated => DocumentMetadata.DateCreated;

        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public string? SourceReferenceNumber => DocumentMetadata.DataSourceReferenceNumber;

        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public bool HasFile => DocumentFile != null;

        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public DocumentImage? RepresentativeImage { get; private set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public string? TerminalDisclaimer { get; private set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public string? AssigneeName { get; private set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public string? Authors { get; private set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public IList<DocumentClassification> DocumentClassifications { get; private set; } = new List<DocumentClassification>();

        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public IList<DocumentTranslation> DocumentTranslations { get; private set; } = new List<DocumentTranslation>();

        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public DocumentTranslation? DefaultTranslation { get; private set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public string? UserNotes { get; private set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public string? UserUrl { get; private set; }

        #endregion

        public void SetPartitionKey()
        {
            PartitionKey = new PartitionKeyBuilder()
                .Add(ReferenceNumber)
                .Add(Id)
                .Build();
        }

        public void InitRepresentativeImage()
        {
            RepresentativeImage = DocumentImages.FirstOrDefault(i => i.IsRepresentative);
        }

        public void SetDocumentData(DocumentData? documentData)
        {
            DocumentData = documentData;

            UserNotes = documentData?.UserNotes;
            UserUrl = documentData?.UserUrl;
            TerminalDisclaimer = documentData?.TerminalDisclaimer;
            AssigneeName = documentData?.AssigneeName;
            Authors = DocumentData?.Authors;
            DocumentClassifications = documentData?.DocumentClassifications ?? new List<DocumentClassification>();
            DocumentTranslations = DocumentData?.DocumentTranslations ?? new List<DocumentTranslation>();
            DefaultTranslation = DocumentData?.DefaultTranslation;
        }

        public void InitDocument(
            DocumentType documentType,
            int? patentFamilyId,
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
            DateTime? pctFilingDate)
        {
            DocumentType = documentType;
            PatentFamilyId = patentFamilyId;
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
        }

        public void SetDocumentType(DocumentType documentType)
        {
            DocumentType = documentType;
        }

        public void SetDocumentImportStatus(QueueStatus status)
        {
            ImportStatus ??= new ImportMetadata();

            ImportStatus.SetStatus(status);
        }

        public void SetApplicationNumber(string value)
        {
            if (!ApplicationNumber.Compare(value))
            {
                ApplicationNumber = value;
            }
        }

        public void SetPublicationNumber(string value)
        {
            if (!PublicationNumber.Compare(value))
            {
                PublicationNumber = value;
            }
        }

        public void SetPatentNumber(string value)
        {
            if (!PatentNumber.Compare(value))
            {
                PatentNumber = value;
            }
        }

        public void SetPctNumber(string value)
        {
            if (!PctNumber.Compare(value))
            {
                PctNumber = value;
            }
        }

        public void SetDocumentDataFile(string blobPath)
        {
            DocumentDataBlobPath = blobPath;
        }

        public void InitPairData()
        {
            PairData ??= new ();
        }

        public void SetDocumentFile(DocumentFile documentFile)
        {
            DocumentFile = documentFile;
        }

        public void RemoveDocumentFile()
        {
            DocumentFile = null;
        }

        public void AddDocumentImage(DocumentImage image)
        {
            DocumentImages.Add(image);
        }

        public void RemoveDocumentImage(DocumentImage image)
        {
            DocumentImages.Remove(image);
        }

        public void AddFamilySimples(string familyItem)
        {
            FamilySimples.Add(familyItem);
        }

        public void AddFamilyExtendeds(string familyItem)
        {
            FamilyExtendeds.Add(familyItem);
        }

        public void SetTermAdjustment(int? value)
        {
            if (TermAdjustment != value)
            {
                TermAdjustment = value;
            }
        }

        public void SetEarliestPriorityNumber(string? value)
        {
            if (!EarliestPriorityNumber.Compare(value))
            {
                EarliestPriorityNumber = value;
            }
        }

        public void SetEarliestPriorityDate(DateTime? date)
        {
            if (!EarliestPriorityDate.Equals(date))
            {
                EarliestPriorityDate = date;
            }
        }

        public void SetApplicationDate(DateTime? date)
        {
            if (!ApplicationDate.Equals(date))
            {
                ApplicationDate = date;
            }
        }

        public void SetPublicationDate(DateTime? date)
        {
            if (!PublicationDate.Equals(date))
            {
                PublicationDate = date;
            }
        }

        public void SetDateFiled(DateTime? date)
        {
            if (!DateFiled.Equals(date))
            {
                DateFiled = date;
            }
        }

        public void SetIssueDate(DateTime? date)
        {
            if (!IssueDate.Equals(date))
            {
                IssueDate = date;
            }
        }

        public void SetExpirationDate(DateTime? date)
        {
            if (!ExpirationDate.Equals(date))
            {
                ExpirationDate = date;
            }
        }

        public void SetOriginalExpirationDate(DateTime? date)
        {
            if (!OriginalExpirationDate.Equals(date))
            {
                OriginalExpirationDate = date;
            }
        }

        public void SetAdjustedExpirationDate(DateTime? date)
        {
            if (!AdjustedExpirationDate.Equals(date))
            {
                AdjustedExpirationDate = date;
            }
        }

        public void SetPctFilingDate(DateTime? date)
        {
            if (!PctFilingDate.Equals(date))
            {
                PctFilingDate = date;
            }
        }

        public void SetDocumentDataSource(DataSource source, string? dataSourceReferenceNumber = null)
        {
            DocumentMetadata.SetDataSource(source);

            if (!string.IsNullOrEmpty(dataSourceReferenceNumber))
            {
                DocumentMetadata.SetDataSourceReferenceNumber(dataSourceReferenceNumber);
            }
        }
    }
}