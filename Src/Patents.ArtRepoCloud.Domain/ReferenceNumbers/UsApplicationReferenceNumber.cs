using Patents.ArtRepoCloud.Domain.Enums;

namespace Patents.ArtRepoCloud.Domain.ReferenceNumbers
{
    public class UsApplicationReferenceNumber : Domain.ReferenceNumbers.ReferenceNumber
    {
        public UsApplicationReferenceNumber(string countryCode, string number, string kindCode, string separatorFormat, string sourceReferenceNumber) 
            : base(countryCode, number, kindCode, separatorFormat, sourceReferenceNumber, ReferenceNumberSourceType.UsApplication)
        {
            
        }

        public override string ToString() { return $"{CountryCode}{Number}"; }
    }
}