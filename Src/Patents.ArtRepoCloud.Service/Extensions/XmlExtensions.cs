using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Patents.ArtRepoCloud.Service.Extensions
{
    public static class XmlExtensions
    {
        public static XElement? Find(this XElement element, string elementName, Func<XElement, bool> func)
        {
            return element.Descendants(elementName).FirstOrDefault(func);
        }

        public static XElement? FirstOrDefault(this XElement? element, string elementName)
        {
            return element?.Descendants(elementName).FirstOrDefault();
        }

        public static DateTime? ParseDate(this string? value, string format)
        {
            return string.IsNullOrEmpty(value) ? null : DateTime.ParseExact(value, format, CultureInfo.InvariantCulture);
        }

        public static string InnerXml(this XElement x)
        {
            return Regex.Replace(x.ToString().Trim(),
                string.Format(@"^</?[^>]*{0}[^>]*>|</?[^>]*{0}[^>]*>$", x.Name),
                "").Trim();
        }
    }
}