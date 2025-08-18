using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.DataProviders.Uspto.Contracts;
using Patents.ArtRepoCloud.Service.DataProviders.Uspto.Contracts.AttorneyAgents;
using Patents.ArtRepoCloud.Service.Factories.HttpFactory.Interfaces;
using Patents.ArtRepoCloud.Service.ValueObjects;

namespace Patents.ArtRepoCloud.Service.DataProviders.Uspto.Interfaces
{
    public interface IUsptoApiProxy
    {
        Task<IRequestResult<ApplicationDataContract?>> GetApplicationData(PairNumber pairNumber, CancellationToken cancellationToken);
        Task<IRequestResult<ContinuityDataContract?>> GetContinuity(PairNumber pairNumber, CancellationToken cancellationToken);
        Task<IRequestResult<TransactionHistoryDataContract>> GetTransactionHistory(PairNumber pairNumber, CancellationToken cancellationToken);
        Task<IRequestResult<DocumentFilesInfoDataContract?>> GetDocumentsInfo(PairNumber pairNumber, CancellationToken cancellationToken);
        Task<IRequestResult<AttorneyAgentsContract?>> GetAttorneyAgents(PairNumber pairNumber, CancellationToken cancellationToken);
        Task<IRequestResult<PostDocumentsRequestResponseDataContract?>> PostFilesRequestAsync(PairNumber pairNumber, PostDocumentsRequestRequest request, CancellationToken cancellationToken);
        Task<IRequestResult<Stream?>> DownloadFiles(PairNumber pairNumber, string requestIdentifier, CancellationToken cancellationToken);
    }
}