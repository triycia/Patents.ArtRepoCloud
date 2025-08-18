namespace Patents.ArtRepoCloud.Service.DataFetchers
{
    public interface IFetcher<in TRequest, TResponse>
    {
        Task<TResponse> ProcessAsync(TRequest request, CancellationToken cancellationToken);
    }
}