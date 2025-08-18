namespace Patents.ArtRepoCloud.Service.ValueObjects.PairData
{
    public class CorrespondenceAddressDto
    {
        public CorrespondenceAddressDto(string customerNumber, AddressDto customerAddress)
        {
            CustomerNumber = customerNumber;
            CustomerAddress = customerAddress;
        }

        public string CustomerNumber { get; }
        public AddressDto CustomerAddress { get; }
    }
}