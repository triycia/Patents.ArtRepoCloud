using Patents.ArtRepoCloud.Domain.Enums;

namespace Patents.ArtRepoCloud.Domain.ReferenceNumbers
{
    public class OtherReferenceNumber : Domain.ReferenceNumbers.ReferenceNumber
    {
        public OtherReferenceNumber(string countryCode, string number, string kindCode, string separatorFormat, string sourceReferenceNumber) 
            : base(countryCode, number, kindCode, separatorFormat, sourceReferenceNumber, ReferenceNumberSourceType.Other)
        {
            
        }
    }
}