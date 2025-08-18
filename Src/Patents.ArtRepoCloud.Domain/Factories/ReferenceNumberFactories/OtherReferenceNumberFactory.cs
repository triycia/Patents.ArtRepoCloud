using System.Text.RegularExpressions;
using Patents.ArtRepoCloud.Domain.Factories.Interfaces;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;

namespace Patents.ArtRepoCloud.Domain.Factories.ReferenceNumberFactories
{
    public class OtherReferenceNumberFactory : ReferenceNumberAbstractFactory, IReferenceNumberFactory
    {
        private const string RegExp = @"^(?<CountryCode>[A-Za-z][A-Za-z])(?<!(EP|US|WO))(?<Number>[\dA-Z]+)(?<KindCode>[A-Z]\d?)$";

        public ReferenceNumber? Parse(string referenceNumber)
        {
            var formattedReferenceNumber = Format(referenceNumber);
            var match = Regex.Match(formattedReferenceNumber, RegExp);

            if (!match.Success)
                return null;

            var countryCode = match.Groups["CountryCode"].Value;
            var number = match.Groups["Number"].Value;
            var kindCode = match.Groups["KindCode"].Value;
            var separatorFormat = string.Empty;

            if (string.IsNullOrEmpty(countryCode) || string.IsNullOrEmpty(number) || string.IsNullOrEmpty(kindCode))
                return null;

            return new OtherReferenceNumber(countryCode, number, kindCode, separatorFormat, referenceNumber);
        }
    }
}