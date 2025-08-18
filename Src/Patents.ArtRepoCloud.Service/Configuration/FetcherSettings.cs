using Patents.ArtRepoCloud.Service.Code.Attributes;
using Patents.ArtRepoCloud.Service.DataFetchers.Epo;
using Patents.ArtRepoCloud.Service.DataFetchers.Ifi;
using Patents.ArtRepoCloud.Service.DataFetchers.IpdIfi;
using Patents.ArtRepoCloud.Service.DataFetchers.Questel;
using Patents.ArtRepoCloud.Service.DataFetchers.Uspto;

namespace Patents.ArtRepoCloud.Service.Configuration
{
    public class FetcherSettings
    {
        [Fetcher(typeof(IpdIfiDocumentFetcher))]
        public bool EnableIpdIfiDocumentProvider { get; init; }

        [Fetcher(typeof(IpdIfiImageFetcher))]
        public bool EnableIpdIfiImageFetcher { get; init; }

        [Fetcher(typeof(IfiDocumentFetcher))]
        public bool EnableIfiDocumentProvider { get; init; }

        [Fetcher(typeof(IfiPdfFetcher))]
        public bool EnableIfiPdfProvider { get; init; }

        [Fetcher(typeof(IfiImageFetcher))]
        public bool EnableIfiImageProvider { get; init; }

        [Fetcher(typeof(EpoDocumentFetcher))]
        public bool EnableEpoDocumentProvider { get; init; }

        [Fetcher(typeof(EpoPdfFetcher))]
        public bool EnableEpoPdfProvider { get; init; }

        [Fetcher(typeof(EpoImageFetcher))]
        public bool EnableEpoImageProvider { get; init; }

        [Fetcher(typeof(QuestelPdfFetcher))]
        public bool EnableQuestelPdfProvider { get; init; }

        [Fetcher(typeof(PairDocumentFetcher))]
        public bool EnableUsptoDocumentProvider { get; init; }
    }
}