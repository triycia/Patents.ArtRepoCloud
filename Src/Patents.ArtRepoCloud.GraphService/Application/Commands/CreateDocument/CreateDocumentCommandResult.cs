namespace Patents.ArtRepoCloud.GraphService.Application.Commands.CreateDocument
{
    public class CreateDocumentCommandResult
    {
        public CreateDocumentCommandResult(string referenceNumber, bool isSuccess)
        {
            ReferenceNumber = referenceNumber;
            IsSuccess = isSuccess;
        }

        public string ReferenceNumber { get; }
        public bool IsSuccess { get; }
    }
}