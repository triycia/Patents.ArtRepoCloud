using Patents.ArtRepoCloud.Domain.Enums;

namespace Patents.ArtRepoCloud.Domain.ReferenceNumbers
{
    public class ReferenceNumber : IReferenceNumber
    {
        public ReferenceNumber(
            string countryCode, 
            string number, 
            string? kindCode, 
            string separatorFormat, 
            string sourceReferenceNumber, 
            ReferenceNumberSourceType numberType)
        {
            CountryCode = countryCode;
            Number = number;
            KindCode = kindCode;
            SeparatorFormat = separatorFormat;
            SourceReferenceNumber = sourceReferenceNumber;
            NumberType = numberType;
        }

        /// <summary>
        /// Gets the orginal reference number that was used to create this <see cref="ReferenceNumber"/>.
        /// </summary>
        public string SourceReferenceNumber { get; init; }

        public ReferenceNumberSourceType NumberType { get; }

        /// <summary>
        /// Gets the country code of the reference number.
        /// </summary>
        public string CountryCode { get; }

        /// <summary>
        /// Gets the numeric portion of the reference number.
        /// </summary>
        public string Number { get; }

        /// <summary>
        /// Gets the kind code of the reference number.
        /// </summary>
        public string? KindCode { get; private set; }

        /// <summary>
        /// Gets the separator format of the reference number.
        /// </summary>
        public string SeparatorFormat { get; }

        /// <summary>
        /// Gets the separator format of the reference number.
        /// </summary>
        public string Ucid => $"{CountryCode}-{Number}{(string.IsNullOrEmpty(KindCode) ? "" : $"-{KindCode}")}";

        public bool IsUs() => NumberType == ReferenceNumberSourceType.UsPatent || NumberType == ReferenceNumberSourceType.UsApplication;
        public override string ToString() => $"{CountryCode}{Number}{KindCode}";
        public void SetKindCode(string kindCode) => KindCode = kindCode;
        
    }
}