namespace Patents.ArtRepoCloud.Domain.Settings
{
    public interface IDirectorySettings
    {
        string ArtDirectory { get; }
        string TempDirectory { get; }
        string JsonDirectory { get; }
        public string ZipDirectory { get; }
        bool EnableTimestamp { get; }
    }
}