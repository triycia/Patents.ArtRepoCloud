using Patents.ArtRepoCloud.Service.ValueObjects.BibData;

namespace Patents.ArtRepoCloud.Service.DataProviders.IpdIfi.Contracts
{
    public class IpdIfiDocumentResult
    {
        public IpdIfiDocumentResult(BibDocument? document, IpdIfiRequestStatus status)
        {
            Document = document;
            Status = status;
        }

        public BibDocument? Document { get; }
        public IpdIfiRequestStatus Status { get; }
    }
}