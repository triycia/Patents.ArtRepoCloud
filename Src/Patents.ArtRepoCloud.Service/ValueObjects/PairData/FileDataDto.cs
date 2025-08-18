namespace Patents.ArtRepoCloud.Service.ValueObjects.PairData
{
    public class FileDataDto
    {
        public FileDataDto(
            int pageCount,
            string? category,
            string? documentCode,
            string? documentDescription,
            string? objectId,
            DateTime mailRoomDate,
            IEnumerable<string> mimeTypes,
            int logicalPackageNumber,
            int ifwCheckboxIndex)
        {
            PageCount = pageCount;
            Category = category;
            DocumentCode = documentCode;
            DocumentDescription = documentDescription;
            ObjectId = objectId;
            MailRoomDate = mailRoomDate;
            MimeTypes = mimeTypes;
            LogicalPackageNumber = logicalPackageNumber;
            IFWCheckboxIndex = ifwCheckboxIndex;
        }

        public int PageCount { get; }
        public string? Category { get; }
        public string? DocumentCode { get; }
        public string? DocumentDescription { get; }
        public string? ObjectId { get; }
        public DateTime MailRoomDate { get; }
        public IEnumerable<string> MimeTypes { get; }
        public int LogicalPackageNumber { get; }
        public int IFWCheckboxIndex { get; }
    }
}