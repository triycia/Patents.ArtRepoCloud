using Newtonsoft.Json;

namespace Patents.ArtRepoCloud.Service.DataProviders.Uspto.RequestFactory.ValueObject
{
    public class UsptoToken
    {
        public UsptoToken() {}
        public UsptoToken(double expiration, double tokenDateTime)
        {
            Expiration = expiration;
            TokenDateTime = tokenDateTime;
        }

        [JsonProperty(PropertyName = "iat")]
        public double TokenDateTime { get; }

        [JsonProperty(PropertyName = "exp")]
        public double Expiration { get; }


        public bool IsValid()
        {
            DateTime expDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            expDateTime = expDateTime.AddSeconds(Expiration).ToLocalTime();

            return DateTime.Now < expDateTime;
        }
    }
}