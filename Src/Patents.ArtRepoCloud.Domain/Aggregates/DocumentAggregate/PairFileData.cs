namespace Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate
{
    public class PairFileData
    {
        public PairFileData(){}
        public PairFileData(
            string documentCode, 
            string documentDescription, 
            string category, 
            string ifwCheckboxIndex, 
            string objectId,  
            int pageCount,
            DateTime mailroomDate,
            DateTime dateChanged, 
            DateTime createdDate)
        {
            DocumentCode = documentCode;
            DocumentDescription = documentDescription;
            Category = category;
            IfwCheckboxIndex = ifwCheckboxIndex;
            ObjectId = objectId;
            PageCount = pageCount;
            MailroomDate = mailroomDate;
            DateChanged = dateChanged;
            CreatedDate = createdDate;
        }

        public string DocumentCode { get; private set; }
        public string DocumentDescription { get; private set; }
        public string Category { get; private set; }
        public string IfwCheckboxIndex { get; private set; }
        public string ObjectId { get; private set; }
        public int PageCount { get; private set; }
        public DateTime MailroomDate { get; private set; }
        public DateTime DateChanged { get; private set; }
        public DateTime CreatedDate { get; private set; }

        public DocumentFile? PairFile { get; private set; }

        public void SetDocumentFile(DocumentFile file)
        {
            PairFile = file;
        }
    }
}