using Patents.ArtRepoCloud.Service.AWS;

namespace Patents.ArtRepoCloud.Service.Services.Interfaces
{
    public interface IAwsClientService
    {
        Task<List<IProxyInstance>> GetAllInstances(CancellationToken cancellationToken = default);
        Task<IProxyInstance> GetAvailableInstance(CancellationToken cancellationToken = default);
        Task<IInstance> GetInstance(Guid id, CancellationToken cancellationToken);
        Task<bool> Start(IInstance grabber, CancellationToken cancellationToken);
        Task<bool> Stop(IInstance grabber, CancellationToken cancellationToken);
    }
}