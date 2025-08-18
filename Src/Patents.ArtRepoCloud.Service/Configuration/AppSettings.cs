namespace Patents.ArtRepoCloud.Service.Configuration
{
    public class AppSettings
    {
        public int MaxConcurrentCalls { get; init; }
        public bool EnableFamilyImport { get; init; }
        public bool EnableExtendedFamilyImport { get; init; }
        public string Environment { get; init; }
    }
}