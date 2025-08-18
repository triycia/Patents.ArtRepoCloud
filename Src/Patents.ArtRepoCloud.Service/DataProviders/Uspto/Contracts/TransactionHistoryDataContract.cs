using Newtonsoft.Json;

namespace Patents.ArtRepoCloud.Service.DataProviders.Uspto.Contracts
{
    [Serializable]
    public class TransactionHistoryDataContract
    {
        public TransactionHistoryDataContract(string applicationNumber, IEnumerable<TransactionHistory> transactionHistoryList)
        {
            ApplicationNumber = applicationNumber;
            TransactionHistoryList = transactionHistoryList;
        }

        [JsonProperty(PropertyName = "applicationNumberText")]
        public string ApplicationNumber { get; }

        [JsonProperty(PropertyName = "transactionHistoryBag")]
        public IEnumerable<TransactionHistory> TransactionHistoryList { get; }

        public class TransactionHistory
        {
            public TransactionHistory(
                string auditData,
                string caseActionCode,
                string caseActionDescriptionText,
                DateTime recordedDate,
                string sequenceNumber,
                string entityName,
                string publicIndicator,
                string applicationStatusNumber,
                string applicationStatusDescriptionText,
                string applicationStatusDate)
            {
                AuditData = auditData;
                CaseActionCode = caseActionCode;
                CaseActionDescriptionText = caseActionDescriptionText;
                RecordedDate = recordedDate;
                SequenceNumber = sequenceNumber;
                EntityName = entityName;
                PublicIndicator = publicIndicator;
                ApplicationStatusNumber = applicationStatusNumber;
                ApplicationStatusDescriptionText = applicationStatusDescriptionText;
                ApplicationStatusDate = applicationStatusDate;
            }

            public string AuditData { get; }
            public string CaseActionCode { get; }
            public string CaseActionDescriptionText { get; }
            public DateTime RecordedDate { get; }
            public string SequenceNumber { get; }
            public string EntityName { get; }
            public string PublicIndicator { get; }
            public string ApplicationStatusNumber { get; }
            public string ApplicationStatusDescriptionText { get; }
            public string ApplicationStatusDate { get; }
        }
    }
}