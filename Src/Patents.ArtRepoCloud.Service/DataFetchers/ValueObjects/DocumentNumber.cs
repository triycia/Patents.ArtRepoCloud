namespace Patents.ArtRepoCloud.Service.DataFetchers.ValueObjects
{
    public class DocumentNumber
    {
        public DocumentNumber(string countryCode, string number, string kindCode)
        {
            CountryCode = countryCode;
            Number = number;
            KindCode = kindCode;
        }

        public string CountryCode { get; }
        public string Number { get; }
        public string KindCode { get; }

        public override string ToString() => $"{CountryCode}{Number}{KindCode}";
    }
}