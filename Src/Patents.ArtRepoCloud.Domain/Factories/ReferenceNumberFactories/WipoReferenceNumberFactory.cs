using System.Text.RegularExpressions;
using Patents.ArtRepoCloud.Domain.Factories.Interfaces;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;

namespace Patents.ArtRepoCloud.Domain.Factories.ReferenceNumberFactories
{
    public class WipoReferenceNumberFactory : ReferenceNumberAbstractFactory, IReferenceNumberFactory
    {
        public ReferenceNumber? Parse(string referenceNumber)
        {
            var formattedReferenceNumber = Format(referenceNumber);

            var hasInvalidCharacters = HasInvalidCharacters(formattedReferenceNumber);
            if (hasInvalidCharacters)
                return null;

            var match = Regex.Match(formattedReferenceNumber, "WO(?<digits>[0-9]*)(?<kindcode>.*)");

            if (!match.Success)
                return null;

            var countryCode = "WO";
            var kindCodeGroup = match.Groups["kindcode"];
            var kindCodeValue = kindCodeGroup.Value;

            string yearStr;
            string numberStr;

            var digits = match.Groups["digits"].Value;
            var digitsLength = digits.Length;

            if (digitsLength < 2)
                return null;

            if (digitsLength > 8)
            {
                numberStr = digits.Substring(4);
                yearStr = digits.Substring(2, 2);
            }
            else
            {
                numberStr = digits.Substring(2);
                yearStr = digits.Substring(0, 2);
            }

            var year = int.Parse(yearStr);
            yearStr = (year < 78 ? "20" : "19") + yearStr;

            var number = $"{yearStr}{numberStr.PadLeft(6, '0')}";

            var separatorFormatKindCode = string.IsNullOrEmpty(kindCodeValue) ? string.Empty : $".{kindCodeValue}";
            var separatorFormat = $"{countryCode}.{number}{separatorFormatKindCode}";

            return new WipoReferenceNumber(countryCode, number, kindCodeValue, separatorFormat, referenceNumber);
        }
    }
}