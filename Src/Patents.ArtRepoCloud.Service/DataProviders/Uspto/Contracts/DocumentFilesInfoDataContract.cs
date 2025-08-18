using Newtonsoft.Json;

namespace Patents.ArtRepoCloud.Service.DataProviders.Uspto.Contracts
{
    public class DocumentFilesInfoDataContract
    {
        public DocumentFilesInfoDataContract(IEnumerable<ApplicationDocumentsMetadata> result, IEnumerable<string> error)
        {
            Result = result;
            Error = error;
        }

        [JsonProperty(PropertyName = "resultBag")]
        public IEnumerable<ApplicationDocumentsMetadata> Result { get; }

        [JsonProperty(PropertyName = "errorBag")]
        public IEnumerable<string> Error { get; }


        public class ApplicationDocumentsMetadata
        {
            public ApplicationDocumentsMetadata(string applicationNumber, IEnumerable<Document> documents)
            {
                ApplicationNumber = applicationNumber;
                Documents = documents;
            }

            [JsonProperty(PropertyName = "applicationNumberText")]
            public string ApplicationNumber { get; }

            [JsonProperty(PropertyName = "documentBag")]
            public IEnumerable<Document> Documents { get; }


            public class Document
            {
                public Document(
                    int pageTotalQuantity,
                    string documentIdentifier,
                    string directionCategory,
                    string documentCode,
                    string documentDescription,
                    DateTime officialDate,
                    int logicalPackageNumber,
                    string[] mimeTypeBag)
                {
                    PageTotalQuantity = pageTotalQuantity;
                    DocumentIdentifier = documentIdentifier;
                    DirectionCategory = directionCategory;
                    DocumentCode = documentCode;
                    DocumentDescription = documentDescription;
                    OfficialDate = officialDate;
                    LogicalPackageNumber = logicalPackageNumber;
                    MimeTypeBag = mimeTypeBag;
                }

                [JsonProperty(PropertyName = "pageTotalQuantity")]
                public int PageTotalQuantity { get; }

                [JsonProperty(PropertyName = "documentIdentifier")]
                public string DocumentIdentifier { get; }

                [JsonProperty(PropertyName = "directionCategory")]
                public string DirectionCategory { get; }

                [JsonProperty(PropertyName = "documentCode")]
                public string DocumentCode { get; }

                [JsonProperty(PropertyName = "documentDescription")]
                public string DocumentDescription { get; }

                [JsonProperty(PropertyName = "officialDate")]
                public DateTime OfficialDate { get; }

                [JsonProperty(PropertyName = "logicalPackageNumber")]
                public int LogicalPackageNumber { get; }

                [JsonProperty(PropertyName = "mimeTypeBag")]
                public string[] MimeTypeBag { get; }
            }
        }
    }
}