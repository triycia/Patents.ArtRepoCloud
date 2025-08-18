using System.Text.RegularExpressions;
using Patents.ArtRepoCloud.Domain.Factories.Interfaces;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;

namespace Patents.ArtRepoCloud.Domain.Factories.ReferenceNumberFactories
{
    public class UsApplicationReferenceNumberFactory : ReferenceNumberAbstractFactory, IReferenceNumberFactory
    {
        private const string RegExp = @"\G\s*(?<CountryCode>US)?(?<NumberWithSlash>\d{4}\/\d{6,7})?(?<NumberWithoutSlash>\d{10,11})?(?<KindCode>[A-Z][A-Z0-9]?)?\s*$";

        public ReferenceNumber? Parse(string referenceNumber)
        {
            var formattedReferenceNumber = Format(referenceNumber);
            var match = Regex.Match(formattedReferenceNumber, RegExp, RegexOptions.IgnoreCase);

            if (!match.Success)
                return null;

            var countryCode = match.Groups["CountryCode"].Value.Equals(string.Empty) ? "US" : match.Groups["CountryCode"].Value;
            var numberWithSlash = match.Groups["NumberWithSlash"].Value;
            var numberWithoutSlash = match.Groups["NumberWithoutSlash"].Value;
            var number = numberWithoutSlash.Equals(string.Empty) ? numberWithSlash.Replace("/", string.Empty) : numberWithoutSlash;
            if (number.Length == 10)
            {
                number = number.Insert(4, "0");
            }
            var kindCode = match.Groups["KindCode"].Value;
            var separatorFormat = string.Empty;

            if (string.IsNullOrEmpty(countryCode) || string.IsNullOrEmpty(number))
                return null;

            return new UsApplicationReferenceNumber(countryCode, number, kindCode, separatorFormat, referenceNumber);
        }
    }
}