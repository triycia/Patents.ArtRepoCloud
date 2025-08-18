using Patents.ArtRepoCloud.Domain.Settings;

namespace Patents.ArtRepoCloud.Service.Configuration
{
    public class DirectorySettings : IDirectorySettings
    {
        public string SubFolder { get; init; }
        public string ArtDirectory { get; init; }
        public string TempDirectory { get; init; }
        public string JsonDirectory { get; init; }
        public string ZipDirectory { get; init; }
        public bool EnableTimestamp { get; init; }
    }
}