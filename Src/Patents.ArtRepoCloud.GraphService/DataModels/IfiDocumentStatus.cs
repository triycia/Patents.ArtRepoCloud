namespace Patents.ArtRepoCloud.GraphService.DataModels
{
    public class IfiDocumentStatus
    {
        public IfiDocumentStatus(
            string patentStatus,
            string patentLastLegalEvent,
            DateTime? patentLastLegalEventDate,
            string pteStatus,
            string productName,
            string extensionRequested,
            DateTime? pteStatusDate,
            DateTime? expiryDate)
        {
            PatentStatus = patentStatus;
            PatentLastLegalEvent = patentLastLegalEvent;
            PatentLastLegalEventDate = patentLastLegalEventDate;
            ProductName = productName;
            ExtensionRequested = extensionRequested;
            PteStatusDate = pteStatusDate;
            PteStatus = pteStatus;
            ProductName = productName;
            ExtensionRequested = extensionRequested;
            PteStatusDate = pteStatusDate;
            ExpiryDate = expiryDate;
        }

        public string PatentStatus { get; }
        public string PatentLastLegalEvent { get; }
        public DateTime? PatentLastLegalEventDate { get; }
        public string PteStatus { get; }
        public string ProductName { get; }
        public string ExtensionRequested { get; }
        public DateTime? PteStatusDate { get; }
        public DateTime? ExpiryDate { get; }
    }
}