using Patents.ArtRepoCloud.Domain.Settings;

namespace Patents.ArtRepoCloud.GraphService.Configuration
{
    public class DirectorySettings : IDirectorySettings
    {
        public string ArtDirectory { get; init; }
        public string TempDirectory { get; init; }
        public string JsonDirectory { get; init; }
        public string ZipDirectory { get; init; }
        public bool EnableTimestamp { get; init; }
    }
}