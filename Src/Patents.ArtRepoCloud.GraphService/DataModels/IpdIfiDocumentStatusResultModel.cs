namespace Patents.ArtRepoCloud.GraphService.DataModels
{
    public class IpdIfiDocumentStatusResultModel
    {
        public IpdIfiDocumentStatusResultModel(IfiDocumentStatus? documentStatus)
        {
            DocumentStatus = documentStatus;
        }

        public IfiDocumentStatus? DocumentStatus { get; }
    }
}