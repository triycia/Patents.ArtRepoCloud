using Patents.ArtRepoCloud.Domain.Enums;

namespace Patents.ArtRepoCloud.Domain.ReferenceNumbers
{
    public class EpoReferenceNumber : Domain.ReferenceNumbers.ReferenceNumber
    {
        public EpoReferenceNumber(string countryCode, string number, string kindCode, string separatorFormat, string sourceReferenceNumber) 
            : base(countryCode, number, kindCode, separatorFormat, sourceReferenceNumber, ReferenceNumberSourceType.Epo)
        {
            
        }
    }
}