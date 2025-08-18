namespace Patents.ArtRepoCloud.Service.Configuration
{
    public class EpoSettings
    {
        public string EpoServiceUrl { get; init; }
        public string EpoKey { get; init; }
        public string EpoSecret { get; init; }
        public string PublishedDataQueryString { get; init; }
        public string PageLinkQueryString { get; init; }
    }
}