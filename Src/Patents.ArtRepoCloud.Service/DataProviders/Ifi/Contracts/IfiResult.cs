using Newtonsoft.Json;

namespace Patents.ArtRepoCloud.Service.DataProviders.Ifi.Contracts
{
    public class IfiResult
    {
        [JsonProperty("content")]
        public IfiContent Content { get; init; }

        [JsonProperty("attachments")]
        public IEnumerable<IfiAttachment> Attachments { get; init; }
    }
}