using Newtonsoft.Json;

namespace Patents.ArtRepoCloud.Service.DataProviders.Uspto.Contracts.AttorneyAgents
{
    [Serializable]
    public class AddressContract
    {
        public AddressContract(
            string customerNumber,
            CustomerAddressContract customerAddress,
            IEnumerable<CustomerAttorneyContract> customerAttorneys)
        {
            CustomerNumber = customerNumber;
            CustomerAddress = customerAddress;
            CustomerAttorneys = customerAttorneys;
        }
        public string CustomerNumber { get; }

        [JsonProperty(PropertyName = "customerAddress")]
        public CustomerAddressContract CustomerAddress { get; }

        [JsonProperty(PropertyName = "customerAttorneyBag")]
        public IEnumerable<CustomerAttorneyContract> CustomerAttorneys { get; }
    }
}