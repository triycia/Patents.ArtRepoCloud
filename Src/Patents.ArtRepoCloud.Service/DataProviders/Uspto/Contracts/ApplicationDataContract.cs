using Newtonsoft.Json;

namespace Patents.ArtRepoCloud.Service.DataProviders.Uspto.Contracts
{
    [Serializable]
    public class ApplicationDataContract
    {
        public ApplicationDataContract(
            ApplicationData applicationMetaData,
            Examiner examinerDetails,
            List<Continuity> parentContinuities,
            List<Continuity> childContinuities,
            List<Inventor> inventors,
            List<EarliestPublicationData> earliestPublicationDataList, 
            WipoApplication? wipoApplicationData, 
            string status)
        {
            ApplicationMetaData = applicationMetaData;
            ExaminerDetails = examinerDetails;
            ParentContinuities = parentContinuities;
            ChildContinuities = childContinuities;
            Inventors = inventors;
            EarliestPublicationDataList = earliestPublicationDataList;
            WipoApplicationData = wipoApplicationData;
            Status = status;
        }

        public ApplicationData ApplicationMetaData { get; }

        public Examiner ExaminerDetails { get; }

        [JsonProperty(PropertyName = "parentContinuityBag")]
        public List<Continuity> ParentContinuities { get; }

        [JsonProperty(PropertyName = "childContinuityBag")]
        public List<Continuity> ChildContinuities { get; }

        [JsonProperty(PropertyName = "inventorBag")]
        public List<Inventor> Inventors { get; }

        [JsonProperty(PropertyName = "preGrantPublicationData")]
        public List<EarliestPublicationData> EarliestPublicationDataList { get; }

        public WipoApplication? WipoApplicationData { get; }
        public string Status { get; }


        [Serializable]
        public class ApplicationData
        {
            public ApplicationData(
               DateTime? filingDate,
               DateTime? effectiveFilingDate,
               DateTime? grantDate,
               string subclass,
               string groupArtUnitNumber,
               string firstInventorToFileIndicator,
               string inventionSubjectMatterCategory,
               string patentNumber, string statusNumber,
               string nationalStageIndicator,
               string applicationStatusDescriptionText,
               string viewTypeCategory,
               AppIdentification applicationIdentification,
               string customerNumber,
               string @class,
               string docketNumber,
               string applicationConfirmationNumber,
               string inventionTitle,
               DateTime? applicationStatusDate,
               string applicationTypeCategory,
               string status)
            {
                FilingDate = filingDate;
                EffectiveFilingDate = effectiveFilingDate;
                GrantDate = grantDate;
                Subclass = subclass;
                GroupArtUnitNumber = groupArtUnitNumber;
                FirstInventorToFileIndicator = firstInventorToFileIndicator;
                InventionSubjectMatterCategory = inventionSubjectMatterCategory;
                PatentNumber = patentNumber;
                StatusNumber = statusNumber;
                NationalStageIndicator = nationalStageIndicator;
                ApplicationStatusDescriptionText = applicationStatusDescriptionText;
                ViewTypeCategory = viewTypeCategory;
                ApplicationIdentification = applicationIdentification;
                CustomerNumber = customerNumber;
                Class = @class;
                DocketNumber = docketNumber;
                ApplicationConfirmationNumber = applicationConfirmationNumber;
                InventionTitle = inventionTitle;
                ApplicationStatusDate = applicationStatusDate;
                ApplicationTypeCategory = applicationTypeCategory;
                Status = status;
            }

            public DateTime? FilingDate { get; }
            public DateTime? EffectiveFilingDate { get; }
            public DateTime? GrantDate { get; }
            public string Subclass { get; }
            public string GroupArtUnitNumber { get; }
            public string FirstInventorToFileIndicator { get; }
            public string InventionSubjectMatterCategory { get; }
            public string PatentNumber { get; }
            public string StatusNumber { get; }
            public string NationalStageIndicator { get; }
            public string ApplicationStatusDescriptionText { get; }
            public string ViewTypeCategory { get; }
            public AppIdentification ApplicationIdentification { get; }
            public string CustomerNumber { get; }
            public string Class { get; }
            public string DocketNumber { get; }
            public string ApplicationConfirmationNumber { get; }
            public string InventionTitle { get; }
            public DateTime? ApplicationStatusDate { get; }
            public string ApplicationTypeCategory { get; }
            public string Status { get; }

            public class AppIdentification
            {
                public AppIdentification(string applicationNumberText, string publicationNumber)
                {
                    ApplicationNumberText = applicationNumberText;
                    PublicationNumber = publicationNumber;
                }

                public string? ApplicationNumberText { get; }
                public string? PublicationNumber { get; }
            }

            public class Publication
            {
                public Publication(
                    string publicationSequenceNumber,
                    string publicationKindCode,
                    string publicationDate)
                {
                    PublicationSequenceNumber = publicationSequenceNumber;
                    PublicationKindCode = publicationKindCode;
                    PublicationDate = publicationDate;
                }

                public string PublicationSequenceNumber { get; }
                public string PublicationKindCode { get; }
                public string PublicationDate { get; }
            }
        }

        public class Examiner
        {
            public Examiner(
                string givenName,
                string middleName,
                string familyName,
                string phoneNumber)
            {
                GivenName = givenName;
                MiddleName = middleName;
                FamilyName = familyName;
                PhoneNumber = phoneNumber;
            }

            public string GivenName { get; }
            public string MiddleName { get; }
            public string FamilyName { get; }
            public string PhoneNumber { get; }
        }

        public class Continuity
        {
            public Continuity(
                string claimType,
                string patentNumber,
                string publicIndicator,
                string parentApplicationNumberText,
                string childApplicationNumberText,
                DateTime? filingDate,
                string applicationStatusCategory,
                string statusDescriptionText,
                bool aiaIndicator)
            {
                ClaimType = claimType;
                PatentNumber = patentNumber;
                PublicIndicator = publicIndicator;
                ParentApplicationNumberText = parentApplicationNumberText;
                ChildApplicationNumberText = childApplicationNumberText;
                FilingDate = filingDate;
                ApplicationStatusCategory = applicationStatusCategory;
                StatusDescriptionText = statusDescriptionText;
                AiaIndicator = aiaIndicator;
            }

            public string ClaimType { get; }
            public string PatentNumber { get; }
            public string PublicIndicator { get; }
            public string ParentApplicationNumberText { get; }
            public string ChildApplicationNumberText { get; }
            public DateTime? FilingDate { get; }
            public string ApplicationStatusCategory { get; }
            public string StatusDescriptionText { get; }
            public bool AiaIndicator { get; }
        }

        public class Inventor
        {
            public Inventor(
                string nameLineOneText,
                string firstName,
                string lastName,
                string cityName,
                string geographicRegionName)
            {
                NameLineOneText = nameLineOneText;
                FirstName = firstName;
                LastName = lastName;
                CityName = cityName;
                GeographicRegionName = geographicRegionName;
            }

            public string NameLineOneText { get; }
            public string FirstName { get; }
            public string LastName { get; }
            public string CityName { get; }
            public string GeographicRegionName { get; }
        }

        public class EarliestPublicationData
        {
            public EarliestPublicationData(
                string ipOfficeCode,
                string publicationYear,
                string publicationSequenceNumber,
                string patentDocumentKindCode,
                DateTime? publicationDate)
            {
                IpOfficeCode = ipOfficeCode;
                PublicationYear = publicationYear;
                PublicationSequenceNumber = publicationSequenceNumber;
                PatentDocumentKindCode = patentDocumentKindCode;
                PublicationDate = publicationDate;
            }

            public string IpOfficeCode { get; }
            public string PublicationYear { get; }
            public string PublicationSequenceNumber { get; }
            public string PatentDocumentKindCode { get; }
            public DateTime? PublicationDate { get; }
        }

        public class WipoApplication
        {
            public WipoApplication(
                string? internationalPublicationNumber, 
                DateTime? internationalPublicationDate, 
                string? relatedApplicationNumberText)
            {
                InternationalPublicationNumber = internationalPublicationNumber;
                InternationalPublicationDate = internationalPublicationDate;
                RelatedApplicationNumberText = relatedApplicationNumberText;
            }

            public string? InternationalPublicationNumber { get; }
            public DateTime? InternationalPublicationDate { get; }
            public string? RelatedApplicationNumberText { get; }
        }
    }
}