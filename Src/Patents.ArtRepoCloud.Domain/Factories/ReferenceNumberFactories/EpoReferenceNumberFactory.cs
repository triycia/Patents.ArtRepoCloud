using System.Text.RegularExpressions;
using Patents.ArtRepoCloud.Domain.Factories.Interfaces;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;

namespace Patents.ArtRepoCloud.Domain.Factories.ReferenceNumberFactories
{
    public class EpoReferenceNumberFactory : ReferenceNumberAbstractFactory, IReferenceNumberFactory
    {
        private const string RegExp = @"\G\s*(?<CountryCode>EP)(?<Number>\d+)(?<KindCode>[A-Z]{1}\d{1})\s*$";

        public ReferenceNumber? Parse(string referenceNumber)
        {
            var formattedReferenceNumber = Format(referenceNumber);
            var match = Regex.Match(formattedReferenceNumber, RegExp);

            if (!match.Success)
                return null;

            var countryCode = match.Groups["CountryCode"].Value.Equals(string.Empty) ? "EP" : match.Groups["CountryCode"].Value;
            var number = match.Groups["Number"].Value;
            var kindCode = match.Groups["KindCode"].Value;
            var separatorFormatKindCode = string.IsNullOrEmpty(kindCode) ? string.Empty : $".{kindCode}";
            var separatorFormat = $"{countryCode}.{number}{separatorFormatKindCode}";

            if (string.IsNullOrEmpty(countryCode) || string.IsNullOrEmpty(number) || string.IsNullOrEmpty(kindCode))
                return null;

            return new EpoReferenceNumber(countryCode, number, kindCode, separatorFormat, referenceNumber);
        }
    }
}