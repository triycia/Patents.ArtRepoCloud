using Patents.ArtRepoCloud.Service.DataFetchers;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchRequests;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses;

namespace Patents.ArtRepoCloud.Service.Code.Attributes
{
    internal class FetcherAttribute : Attribute
    {
        public Type FetcherType;

        public FetcherAttribute(Type fetcherType)
        {
            if (fetcherType == typeof(IFetcher<IFetchRequest, IFetchResponse>) )
            {
                throw new NotSupportedException($"{fetcherType} is not of type {typeof(IFetcher<IFetchRequest, IFetchResponse>)}");
            }

            FetcherType = fetcherType;
        }
    }
}