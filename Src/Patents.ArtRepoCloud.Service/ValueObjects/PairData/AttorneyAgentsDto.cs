namespace Patents.ArtRepoCloud.Service.ValueObjects.PairData
{
    public class AttorneyAgentsDto
    {
        public AttorneyAgentsDto(
            AddressContractDto? correspondenceAddress, 
            AddressContractDto? powerOfAttorneyAddress)
        {
            CorrespondenceAddress = correspondenceAddress;
            PowerOfAttorneyAddress = powerOfAttorneyAddress;
        }

        public AddressContractDto? CorrespondenceAddress { get; }
        public AddressContractDto? PowerOfAttorneyAddress { get; }
    }
}