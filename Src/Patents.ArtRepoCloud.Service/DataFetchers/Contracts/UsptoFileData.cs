namespace Patents.ArtRepoCloud.Service.DataFetchers.Contracts
{
    public class UsptoFileData : FileData
    {
        public UsptoFileData(
            Guid onDiskId, 
            string fileName, 
            string filePath, 
            string mediaType, 
            long length, 
            string objectId, 
            string documentCode, 
            string category, 
            string documentDescription, 
            string ifwCheckboxIndex, 
            int pageCount,
            DateTime mailRoomDate,
            int sequence = 0) 
                : base(
                onDiskId, 
                fileName, 
                filePath, 
                mediaType, 
                pageCount,
                length, 
                sequence)
        {
            ObjectId = objectId;
            DocumentCode = documentCode;
            Category = category;
            DocumentDescription = documentDescription;
            IFWCheckboxIndex = ifwCheckboxIndex;
            MailRoomDate = mailRoomDate;
        }

        public string ObjectId { get; }
        public string DocumentCode { get; }
        public string Category { get; }
        public string DocumentDescription { get; }
        public string IFWCheckboxIndex { get; }
        public DateTime MailRoomDate { get; }
    }
}