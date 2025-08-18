using Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate;
using Patents.ArtRepoCloud.Service.DataFetchers.ValueObjects;
using Newtonsoft.Json;

namespace Patents.ArtRepoCloud.Service.ValueObjects.BibData
{
    public class BibDocument
    {
        [JsonConstructor]
        public BibDocument(
            string applicationNumber,
            string country,
            int? termAdjustment,
            int? familyId,
            DateTime? earliestPriorityDate,
            DateTime? applicationDate,
            DateTime? publicationDate,
            DateTime? dateFiled,
            DateTime? expirationDate,
            DateTime? originalExpirationDate,
            DateTime? adjustedExpirationDate,
            ICollection<string> familySimpleUcids,
            ICollection<string> familyExtendedUcids,
            string terminalDisclaimer,
            string earliestPriorityNumber,
            string assigneeName,
            string? authors,
            ICollection<BibDocumentTranslation> translations,
            ICollection<BibDocumentClassification> classifications,
            DocumentNumber documentNumber)
        {
            ApplicationNumber = applicationNumber;
            Country = country;
            TermAdjustment = termAdjustment;
            FamilyId = familyId;
            EarliestPriorityDate = earliestPriorityDate;
            ApplicationDate = applicationDate;
            PublicationDate = publicationDate;
            DateFiled = dateFiled;
            ExpirationDate = expirationDate;
            OriginalExpirationDate = originalExpirationDate;
            AdjustedExpirationDate = adjustedExpirationDate;
            FamilySimpleList = familySimpleUcids;
            FamilyExtendedList = familyExtendedUcids;
            TerminalDisclaimer = terminalDisclaimer;
            EarliestPriorityNumber = earliestPriorityNumber;
            AssigneeName = assigneeName;
            Authors = authors;
            Translations = translations;
            Classifications = classifications;
            DocumentNumber = documentNumber;
        }

        public BibDocument(
            string applicationNumber,
            string country,
            int? termAdjustment,
            int? familyId,
            DateTime? earliestPriorityDate,
            DateTime? applicationDate,
            DateTime? publicationDate,
            DateTime? dateFiled,
            DateTime? expirationDate,
            DateTime? originalExpirationDate,
            DateTime? adjustedExpirationDate,
            string terminalDisclaimer,
            string earliestPriorityNumber,
            string assigneeName)
        {
            ApplicationNumber = applicationNumber;
            Country = country;
            TermAdjustment = termAdjustment;
            FamilyId = familyId;
            EarliestPriorityDate = earliestPriorityDate;
            ApplicationDate = applicationDate;
            PublicationDate = publicationDate;
            DateFiled = dateFiled;
            ExpirationDate = expirationDate;
            OriginalExpirationDate = originalExpirationDate;
            AdjustedExpirationDate = adjustedExpirationDate;
            TerminalDisclaimer = terminalDisclaimer;
            EarliestPriorityNumber = earliestPriorityNumber;
            AssigneeName = assigneeName;

            FamilySimpleList = new List<string>();
            FamilyExtendedList = new List<string>();

            Translations = new List<BibDocumentTranslation>();
            Classifications = new List<BibDocumentClassification>();
        }


        public string ApplicationNumber { get; }
        public string Country { get; }
        public int? TermAdjustment { get; }
        public int? FamilyId { get; }
        public DateTime? EarliestPriorityDate { get; }
        public DateTime? ApplicationDate { get; }
        public DateTime? PublicationDate { get; }
        public DateTime? DateFiled { get; }
        public DateTime? ExpirationDate { get; }
        public DateTime? OriginalExpirationDate { get; }
        public DateTime? AdjustedExpirationDate { get; }
        public string? TerminalDisclaimer { get; }
        public string? EarliestPriorityNumber { get; }
        public string AssigneeName { get; }
        public string? Authors { get; }
        public ICollection<string> FamilySimpleList { get; private set; }
        public ICollection<string> FamilyExtendedList { get; private set; }
        public ICollection<BibDocumentTranslation> Translations { get; private set; }
        public ICollection<BibDocumentClassification> Classifications { get; private set; }
        public DocumentNumber? DocumentNumber { get; }

        public void AddFamilySimple(string familySimpleUcid)
        {
            FamilySimpleList.Add(familySimpleUcid);
        }

        public void AddFamilyExtended(string familyExtendedUcid)
        {
            FamilyExtendedList.Add(familyExtendedUcid);
        }

        public void AddTranslation(BibDocumentTranslation translation)
        {
            Translations.Add(translation);
        }

        public void AddClassification(BibDocumentClassification classifications)
        {
            Classifications.Add(classifications);
        }
    }
}