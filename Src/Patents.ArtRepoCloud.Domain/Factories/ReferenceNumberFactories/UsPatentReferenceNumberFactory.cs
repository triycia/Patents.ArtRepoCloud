using System.Text.RegularExpressions;
using Patents.ArtRepoCloud.Domain.Factories.Interfaces;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;

namespace Patents.ArtRepoCloud.Domain.Factories.ReferenceNumberFactories
{
    public class UsPatentReferenceNumberFactory : ReferenceNumberAbstractFactory, IReferenceNumberFactory
    {
        private readonly List<string> otherFormats = new List<string>() { "PP", "RE", "RX", "AI", "D", "T", "H" };
        private const string RegExp = @"^(?<CountryCode>US)?(?<Number>(?:(?:PP|RE|RX|AI|D|T|H)\d{5,7}|\d{1,8}))(?<KindCode>[A-Z][A-Z0-9]?)?$";

        private bool NumberIsInOtherFormat(string number)
        {
            if (string.IsNullOrEmpty(number))
                return false;

            if (number.Length < 2)
                return false;

            return otherFormats.Exists(n => n.Equals(number.Substring(0, 2), StringComparison.CurrentCultureIgnoreCase));
        }

        public ReferenceNumber? Parse(string referenceNumber)
        {
            var formattedReferenceNumber = Format(referenceNumber);
            var match = Regex.Match(formattedReferenceNumber, RegExp, RegexOptions.IgnoreCase);

            if (!match.Success)
                return null;

            var countryCode = match.Groups["CountryCode"].Value.Equals(string.Empty) ? "US" : match.Groups["CountryCode"].Value;
            var number = match.Groups["Number"].Value;
            var kindCode = match.Groups["KindCode"].Value;
            var separatorFormat = string.Empty;

            if ((string.IsNullOrEmpty(countryCode) && !NumberIsInOtherFormat(number)) || string.IsNullOrEmpty(number))
                return null;

            return string.IsNullOrEmpty(number) ? null : new UsPatentReferenceNumber(countryCode, number, kindCode, separatorFormat, referenceNumber);
        }
    }
}