using Newtonsoft.Json;

namespace Patents.ArtRepoCloud.Service.DataProviders.Ifi.Contracts
{
    public class IfiFamilyMembers
    {
        [JsonProperty("family")]
        public Family FamilyMembers { get; init; }

        public class Family
        {
            [JsonProperty("members")]
            public List<string> Members { get; init; }
        }
    }
}
