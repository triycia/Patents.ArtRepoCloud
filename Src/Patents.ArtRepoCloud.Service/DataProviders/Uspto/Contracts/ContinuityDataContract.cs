using Newtonsoft.Json;

namespace Patents.ArtRepoCloud.Service.DataProviders.Uspto.Contracts
{
    [Serializable]
    public class ContinuityDataContract
    {
        public ContinuityDataContract(
            string applicationNumber,
            IEnumerable<Continuity> parentContinuities,
            IEnumerable<Continuity> childContinuities)
        {
            ApplicationNumber = applicationNumber;
            ParentContinuities = parentContinuities;
            ChildContinuities = childContinuities;
        }

        [JsonProperty(PropertyName = "applicationNumberText")]
        public string ApplicationNumber { get; }

        [JsonProperty(PropertyName = "parentContinuityBag")]
        public IEnumerable<Continuity> ParentContinuities { get; }

        [JsonProperty(PropertyName = "childContinuityBag")]
        public IEnumerable<Continuity> ChildContinuities { get; }

        //public Object WipoApplicationData { get; }

        public class Continuity
        {
            public Continuity(
                //string wipoApplicationData,
                string parentApplicationNumberText,
                string childApplicationNumberText,
                string claimTypeCategory,
                DateTime? filingDate,
                int applicationStatusCategory,
                string customerNumber,
                string patentNumber,
                string inventionSubjectMatterCategory,
                string publicIndicator,
                string aiaIndicator)
            {
                //WipoApplicationData = wipoApplicationData;
                ParentApplicationNumberText = parentApplicationNumberText;
                ChildApplicationNumberText = childApplicationNumberText;
                ClaimTypeCategory = claimTypeCategory;
                FilingDate = filingDate;
                ApplicationStatusCategory = applicationStatusCategory;
                CustomerNumber = customerNumber;
                PatentNumber = patentNumber;
                InventionSubjectMatterCategory = inventionSubjectMatterCategory;
                PublicIndicator = publicIndicator;
                AiaIndicator = aiaIndicator;
            }

            //public string WipoApplicationData { get; }
            public string ParentApplicationNumberText { get; }
            public string ChildApplicationNumberText { get; }
            public string ClaimTypeCategory { get; }
            public DateTime? FilingDate { get; }
            public int ApplicationStatusCategory { get; }
            public string CustomerNumber { get; }
            public string PatentNumber { get; }
            public string InventionSubjectMatterCategory { get; }
            public string PublicIndicator { get; }
            public string AiaIndicator { get; }

            public class ApplicationData
            {
                public ApplicationData(
                    string publicationNumber,
                    DateTime? publicationDate)
                {
                    PublicationNumber = publicationNumber;
                    PublicationDate = publicationDate;
                }

                public string PublicationNumber { get; }
                public DateTime? PublicationDate { get; }
            }
        }
    }
}