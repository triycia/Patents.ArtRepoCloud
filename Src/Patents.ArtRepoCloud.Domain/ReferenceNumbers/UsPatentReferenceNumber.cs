using Patents.ArtRepoCloud.Domain.Enums;

namespace Patents.ArtRepoCloud.Domain.ReferenceNumbers
{
    public class UsPatentReferenceNumber : Domain.ReferenceNumbers.ReferenceNumber
    {
        private readonly string[] validPrefixes = new string[7]
        {
            "PP",
            "RE",
            "RX",
            "AI",
            "D",
            "T",
            "H"
        };
        public UsPatentReferenceNumber(string countryCode, string number, string kindCode, string separatorFormat, string sourceReferenceNumber) 
            : base(countryCode, number, kindCode, separatorFormat, sourceReferenceNumber, ReferenceNumberSourceType.UsPatent)
        {
            
        }
        public override string ToString() { return $"{CountryCode}{Number}"; }
    }
}