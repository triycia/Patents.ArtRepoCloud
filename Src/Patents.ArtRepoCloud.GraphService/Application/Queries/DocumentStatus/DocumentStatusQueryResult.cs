namespace Patents.ArtRepoCloud.GraphService.Application.Queries.DocumentStatus
{
    public class DocumentStatusQueryResult
    {
        public DocumentStatusQueryResult(
            string referenceNumber, 
            string patentStatus, 
            string patentLastLegalEvent, 
            DateTime? patentLastLegalEventDate, 
            string pteStatus, 
            string productName, 
            string extensionRequested, 
            DateTime? pteStatusDate, 
            DateTime? expiryDate)
        {
            ReferenceNumber = referenceNumber;
            PatentStatus = patentStatus;
            PatentLastLegalEvent = patentLastLegalEvent;
            PatentLastLegalEventDate = patentLastLegalEventDate;
            ProductName = productName;
            ExtensionRequested = extensionRequested;
            PteStatusDate = pteStatusDate;
            PteStatus = pteStatus;
            ExpiryDate = expiryDate;
            ProductName = productName;
            ExtensionRequested = extensionRequested;
            PteStatusDate = pteStatusDate;
            ExpiryDate = expiryDate;
        }

        public DocumentStatusQueryResult(string referenceNumber)
        {
            ReferenceNumber = referenceNumber;
        }

        public string ReferenceNumber { get; }
        public string? PatentStatus { get; }
        public string? PatentLastLegalEvent { get; }
        public DateTime? PatentLastLegalEventDate { get; }
        public string? PteStatus { get; }
        public string? ProductName { get; }
        public string? ExtensionRequested { get; }
        public DateTime? PteStatusDate { get; }
        public DateTime? ExpiryDate { get; }
    }
}