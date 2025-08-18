using Newtonsoft.Json;

namespace Patents.ArtRepoCloud.Service.DataProviders.Uspto.Contracts
{
    public class ApplicationErrorMessage
    {
        public ApplicationErrorMessage(List<string> errors, string status)
        {
            Errors = errors;
            Status = status;
        }

        [JsonProperty(PropertyName = "errorMessageText")]
        public List<string> Errors { get; }
        public string Status { get; }
    }
}