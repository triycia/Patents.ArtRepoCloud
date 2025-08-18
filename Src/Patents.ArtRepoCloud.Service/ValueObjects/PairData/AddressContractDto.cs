namespace Patents.ArtRepoCloud.Service.ValueObjects.PairData
{
    public class AddressContractDto
    {
        public AddressContractDto(
            string customerNumber,
            AddressDto customerAddress,
            IEnumerable<CustomerAttorneyDto> customerAttorneys)
        {
            CustomerNumber = customerNumber;
            CustomerAddress = customerAddress;
            CustomerAttorneys = customerAttorneys;
        }

        public string CustomerNumber { get; }

        public AddressDto CustomerAddress { get; }

        public IEnumerable<CustomerAttorneyDto> CustomerAttorneys { get; }
    }
}