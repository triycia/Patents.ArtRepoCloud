namespace Patents.ArtRepoCloud.Service.ValueObjects.PairData
{
    public class ApplicationDataDto
    {
        public ApplicationDataDto(
            string applicationNumber,
            string? publicationNumber,
            string titleOfInvention,
            DateTime? filingDate,
            DateTime? effectiveFilingDate,
            DateTime? publicationDate, 
            DateTime? grantDate,
            string applicationType,
            string? examinerName,
            string groupArtUnit,
            string confirmationNumber,
            string attorneyDocketNumber,
            string @class,
            string subclass,
            string? firstNamedInventor,
            string customerNumber,
            string status,
            DateTime? statusDate,
            string? earliestPublicationNumber,
            DateTime? earliestPublicationDate,
            string patentNumber,
            DateTime? issueDateOfPatent, 
            string? location = null,
            DateTime? locationDate = null)
        {
            ApplicationNumber = applicationNumber;
            PublicationNumber = publicationNumber;
            TitleOfInvention = titleOfInvention;
            FilingDate = filingDate;
            EffectiveFilingDate = effectiveFilingDate;
            PublicationDate = publicationDate;
            GrantDate = grantDate;
            ApplicationType = applicationType;
            ExaminerName = examinerName;
            GroupArtUnit = groupArtUnit;
            ConfirmationNumber = confirmationNumber;
            AttorneyDocketNumber = attorneyDocketNumber;
            Class = @class;
            Subclass = subclass;
            FirstNamedInventor = firstNamedInventor;
            CustomerNumber = customerNumber;
            Status = status;
            StatusDate = statusDate;
            EarliestPublicationNumber = earliestPublicationNumber;
            EarliestPublicationDate = earliestPublicationDate;
            PatentNumber = patentNumber;
            IssueDateOfPatent = issueDateOfPatent;
            Location = location;
            LocationDate = locationDate;
        }

        public string ApplicationNumber { get; }
        public string PublicationNumber { get; }
        public string TitleOfInvention { get; }
        public DateTime? FilingDate { get; }
        public DateTime? EffectiveFilingDate { get; }
        public DateTime? PublicationDate { get; }
        public DateTime? GrantDate { get; }
        public string ApplicationType { get; }
        public string? ExaminerName { get; }
        public string GroupArtUnit { get; }
        public string ConfirmationNumber { get; }
        public string AttorneyDocketNumber { get; }
        public string Class { get; }
        public string Subclass { get; }
        public string? FirstNamedInventor { get; }
        public string CustomerNumber { get; }
        public string Status { get; }
        public DateTime? StatusDate { get; }
        public string? EarliestPublicationNumber { get; }
        public DateTime? EarliestPublicationDate { get; }
        public string PatentNumber { get; }
        public DateTime? IssueDateOfPatent { get; }
        public string? Location { get; }
        public DateTime? LocationDate { get; }
    }
}