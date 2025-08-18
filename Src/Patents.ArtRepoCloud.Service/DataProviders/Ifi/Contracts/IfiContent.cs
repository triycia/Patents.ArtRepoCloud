using Newtonsoft.Json;

namespace Patents.ArtRepoCloud.Service.DataProviders.Ifi.Contracts
{
    public class IfiContent
    {
        [JsonProperty("response")]
        public ContentData Response { get; init; }

        public class ContentData
        {
            [JsonProperty("numFound")]
            public int NumFound { get; init; }

            [JsonProperty("docs")]
            public IEnumerable<Doc> Docs { get; init; }

            [JsonProperty("start")]
            public int Start { get; init; }
        }

        public class Doc
        {
            [JsonProperty("ucid")]
            public string Ucid { get; init; }

            [JsonProperty("pd")]
            public string Pd { get; init; }

            /// <summary>
            /// This filed was changed in IFI v2.1
            /// The pn field is further tokenized to improve searching document numbers containing alpha-characters. Please note: this field is no longer a replica of ucid and may contain more than one string when returned in the result set. Those relying on pn as a single identifier should use ucid instead.
            /// </summary>
            [JsonProperty("pn")]
            public string Pn { get; init; }

            [JsonProperty("ad")]
            public string Ad { get; init; }

            [JsonProperty("an")]
            public string An { get; init; }

            [JsonProperty("prid")]
            public string Prid { get; init; }

            [JsonProperty("pri")]
            public IEnumerable<string> Pri { get; init; }

            [JsonProperty("ttl_en")]
            public IEnumerable<string> TtlEn { get; init; }
        }
    }
}