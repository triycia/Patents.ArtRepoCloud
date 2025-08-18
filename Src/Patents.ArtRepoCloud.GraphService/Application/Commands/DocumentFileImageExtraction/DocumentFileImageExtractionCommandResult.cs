namespace Patents.ArtRepoCloud.GraphService.Application.Commands.DocumentFileImageExtraction
{
    public class DocumentFileImageExtractionCommandResult
    {
        public DocumentFileImageExtractionCommandResult(bool isSuccess, Guid? onDiskGuid = null, string? destinationPath = null, int? pageCount = null)
        {
            IsSuccess = isSuccess;
            OnDiskGuid = onDiskGuid;
            DestinationPath = destinationPath;
            PageCount = pageCount;
        }

        public bool IsSuccess { get; }
        public Guid? OnDiskGuid { get; }
        public string? DestinationPath { get; }
        public int? PageCount { get; }
    }
}