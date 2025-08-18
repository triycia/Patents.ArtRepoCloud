using Patents.ArtRepoCloud.Domain.Enums;

namespace Patents.ArtRepoCloud.Domain.ReferenceNumbers
{
    public interface IReferenceNumber
    {
        public string CountryCode { get; }
        public string Number { get; }
        public string KindCode { get; }
        public string SourceReferenceNumber { get; }
        public ReferenceNumberSourceType NumberType { get; }
    }
}