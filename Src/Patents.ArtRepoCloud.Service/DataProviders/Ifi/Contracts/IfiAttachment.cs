using Newtonsoft.Json;

namespace Patents.ArtRepoCloud.Service.DataProviders.Ifi.Contracts
{
    public class IfiAttachment
    {
        [JsonProperty("filename")]
        public string Filename { get; init; }

        [JsonProperty("path")]
        public string Path { get; init; }

        [JsonProperty("media")]
        public string Media { get; init; }

        [JsonProperty("size")]
        public int Size { get; init; }
    }
}