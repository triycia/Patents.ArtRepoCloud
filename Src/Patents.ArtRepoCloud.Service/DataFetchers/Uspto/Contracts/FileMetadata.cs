using Patents.ArtRepoCloud.Domain.Enums;

namespace Patents.ArtRepoCloud.Service.DataFetchers.Uspto.Contracts
{
    public class FileMetadata
    {
        public FileMetadata(
            int pageCount,
            string category,
            string documentCode,
            string documentDescription,
            string objectId,
            DateTime mailRoomDate,
            int logicalPackageNumber,
            int ifwCheckboxIndex,
            string mimeType)
        {
            PageCount = pageCount;
            Category = category;
            DocumentCode = documentCode;
            DocumentDescription = documentDescription;
            ObjectId = objectId;
            MailRoomDate = mailRoomDate;
            LogicalPackageNumber = logicalPackageNumber;
            IFWCheckboxIndex = ifwCheckboxIndex;
            MimeType = mimeType;
        }

        public int PageCount { get; }
        public string Category { get; }
        public string DocumentCode { get; }
        public string DocumentDescription { get; }
        public string ObjectId { get; }
        public DateTime MailRoomDate { get; }
        public int LogicalPackageNumber { get; }
        public int IFWCheckboxIndex { get; }
        public string MimeType { get; }

        public UsptoFileDownloadType? IndividualDownloadType { get; private set; } = UsptoFileDownloadType.PdfZip;

        public void SetIndividualDownloadType(UsptoFileDownloadType downloadType)
        {
            IndividualDownloadType = downloadType;
        }
    }
}