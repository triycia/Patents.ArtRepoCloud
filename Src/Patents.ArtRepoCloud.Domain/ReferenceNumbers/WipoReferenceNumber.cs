using Patents.ArtRepoCloud.Domain.Enums;

namespace Patents.ArtRepoCloud.Domain.ReferenceNumbers
{
    public class WipoReferenceNumber : Domain.ReferenceNumbers.ReferenceNumber
    {
        public WipoReferenceNumber(string countryCode, string number, string kindCode, string separatorFormat, string sourceReferenceNumber) 
            : base(countryCode, number, kindCode, separatorFormat, sourceReferenceNumber, ReferenceNumberSourceType.Wipo)
        {
            
        }
        public override string ToString() { return $"{CountryCode}{Number}"; }
    }
}