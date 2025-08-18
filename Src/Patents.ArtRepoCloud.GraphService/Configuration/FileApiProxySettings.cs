using Vikcher.Framework.IO.FileProxy;

namespace Patents.ArtRepoCloud.GraphService.Configuration
{
    public class FileApiProxySettings : IFileApiProxySettings
    {
        public string BaseAddress { get; init; }
    }
}