namespace Patents.ArtRepoCloud.Service.ValueObjects.PairData
{
    public class AddressDto
    {
        public AddressDto(
            string cityName,
            string regionName,
            string countryName,
            string postalCode,
            string nameLineOneText,
            string nameLineTwoText,
            string addressLineOneText)
        {
            CityName = cityName;
            RegionName = regionName;
            CountryName = countryName;
            PostalCode = postalCode;
            NameLineOneText = nameLineOneText;
            NameLineTwoText = nameLineTwoText;
            AddressLineOneText = addressLineOneText;
        }

        public string CityName { get; }
        public string RegionName { get; }
        public string CountryName { get; }
        public string PostalCode { get; }
        public string NameLineOneText { get; }
        public string NameLineTwoText { get; }
        public string AddressLineOneText { get; }
    }
}