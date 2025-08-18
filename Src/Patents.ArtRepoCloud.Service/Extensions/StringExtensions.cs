using System.Text.RegularExpressions;

namespace Patents.ArtRepoCloud.Service.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveRedundantSpaces(this string source)
        {
            return string.IsNullOrWhiteSpace(source) ? string.Empty : Regex.Replace(source, @"\s+", " ").Trim();
        }
    }
}