using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.Enums;
using System.Text.RegularExpressions;

namespace Patents.ArtRepoCloud.Service.Extensions
{
    public static class ReferenceNumberExtensions
    {
        public static ReferenceDocumentType ToDocumentType(this ReferenceNumber referenceNumber)
        {
            switch (referenceNumber.NumberType)
            {
                case ReferenceNumberSourceType.UsPatent:
                    return ReferenceDocumentType.UsPatent;
                case ReferenceNumberSourceType.UsApplication:
                    return ReferenceDocumentType.UsApplication;
                case ReferenceNumberSourceType.Wipo:
                    return ReferenceDocumentType.Wipo;
                case ReferenceNumberSourceType.Epo:
                    return ReferenceDocumentType.Epo;
                case ReferenceNumberSourceType.Other:
                    return ReferenceDocumentType.SupportDocument;
                default:
                    throw new ArgumentOutOfRangeException(nameof(referenceNumber.NumberType), referenceNumber.NumberType, null);
            }
        }

        public static PairNumber ToPairNumber(this ReferenceNumber referenceNumber)
        {
            var applicationRegExp = @"(^[0-9]{2}[/]{1}[0-9]{3}[,]{1}[0-9]{3}$|^[0-9]{2}[0-9]{3}[0-9]{3}$)";

            var applicationMatch = Regex.Match(referenceNumber.Number, applicationRegExp);

            if (applicationMatch.Success)
                return new PairNumber(referenceNumber.Number, UsptoSearchNumberType.ApplicationNumber);

            var patentRegExp = @"(^[0-9]{1}[,]{1}[0-9]{3}[,]{1}[0-9]{3}$|^[0-9]{2}[,]{1}[0-9]{3}[,]{1}[0-9]{3}$|^[0-9]{2}[0-9]{3}[0-9]{3}$|^[0-9]{1}[0-9]{3}[0-9]{3}$)";

            var patentRegExpMatch = Regex.Match(referenceNumber.Number, patentRegExp);

            if (patentRegExpMatch.Success)
                return new PairNumber(referenceNumber.Number, UsptoSearchNumberType.PatentNumber);

            var pctRegExp = @"(^PCT[/]{1}[A-Za-z]{2}[0-9]{2}[/]{1}[0-9]{5}$|^PCT[/]{1}[A-Za-z]{2}[0-9]{4}[/]{1}[0-9]{6}$|^PCT[A-Za-z]{2}[0-9]{2}[0-9]{5}$|^PCT[A-Za-z]{2}[0-9]{4}[0-9]{6}$)";

            var pctRegExpMatch = Regex.Match(referenceNumber.Number, pctRegExp);

            if (pctRegExpMatch.Success)
                return new PairNumber(referenceNumber.Number, UsptoSearchNumberType.PctNumber);

            var publicationRegExp = @"(^([US|us]+\s)[0-9]{4}[-]{1}([0-9]{7})+\s[A-Za-z]{1}[0-9]$|^[0-9]{4}[-]{1}([0-9]{7})+\s[A-Za-z]{1}[0-9]$|^[0-9]{4}[-]{1}[0-9]{7}$|^([US|us]+\s)[0-9]{4}([0-9]{7})+\s[A-Za-z]{1}[0-9]$|^([0-9]{11})+\s[A-Za-z]{1}[0-9]|^[0-9]{11})";

            var publicationRegExpMatch = Regex.Match(referenceNumber.Number, publicationRegExp);

            if (publicationRegExpMatch.Success)
                return new PairNumber(referenceNumber.Number, UsptoSearchNumberType.PublicationNumber);

            return new PairNumber(referenceNumber.Number, UsptoSearchNumberType.PatentNumber);
        }
    }
}