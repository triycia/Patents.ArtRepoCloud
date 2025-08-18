using Newtonsoft.Json;

namespace Patents.ArtRepoCloud.Service.DataProviders.Uspto.Contracts
{
    public class PostDocumentsRequestRequest
    {
        public PostDocumentsRequestRequest(string title, List<Document> documents)
        {
            Title = title;
            Documents = documents;
        }

        public PostDocumentsRequestRequest(string title, string downloadType, List<Document> documents)
        {
            Title = title;
            DownloadType = downloadType;
            Documents = documents;
        }

        [JsonProperty(PropertyName = "fileTitleText")]
        public string Title { get; }

        [JsonProperty(PropertyName = "downloadType")]
        public string? DownloadType { get; }

        [JsonProperty(PropertyName = "documentInformationBag")]
        public List<Document> Documents { get; }

        public class Document
        {
            public Document(
                string bookmarkTitle,
                string documentIdentifier,
                string applicationNumber,
                string? customerNumber,
                string mailDateTime,
                string documentCode,
                string mimeCategory,
                bool previewFileIndicator,
                string documentCategory)
            {
                BookmarkTitle = bookmarkTitle;
                DocumentIdentifier = documentIdentifier;
                ApplicationNumber = applicationNumber;
                CustomerNumber = customerNumber;
                MailDateTime = mailDateTime;
                DocumentCode = documentCode;
                MimeCategory = mimeCategory;
                PreviewFileIndicator = previewFileIndicator;
                DocumentCategory = documentCategory;
            }

            [JsonProperty(PropertyName = "bookmarkTitleText")]
            public string BookmarkTitle { get; }

            [JsonProperty(PropertyName = "documentIdentifier")]
            public string DocumentIdentifier { get; }

            [JsonProperty(PropertyName = "applicationNumberText")]
            public string ApplicationNumber { get; }

            [JsonProperty(PropertyName = "customerNumber")]
            public string? CustomerNumber { get; }

            [JsonProperty(PropertyName = "mailDateTime")]
            public string MailDateTime { get; }

            [JsonProperty(PropertyName = "documentCode")]
            public string DocumentCode { get; }

            [JsonProperty(PropertyName = "mimeCategory")]
            public string MimeCategory { get; }

            [JsonProperty(PropertyName = "previewFileIndicator")]
            public bool PreviewFileIndicator { get; }

            [JsonProperty(PropertyName = "documentCategory")]
            public string DocumentCategory { get; }
        }
    }
}