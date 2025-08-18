using Newtonsoft.Json;

namespace Patents.ArtRepoCloud.Service.DataProviders.Uspto.Contracts
{
    public class PostDocumentsRequestResponseDataContract
    {
        public PostDocumentsRequestResponseDataContract(string requestIdentifier, string status)
        {
            RequestIdentifier = requestIdentifier;
            Status = status;
        }

        [JsonProperty(PropertyName = "requestIdentifier")]
        public string RequestIdentifier { get; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; }
    }
}