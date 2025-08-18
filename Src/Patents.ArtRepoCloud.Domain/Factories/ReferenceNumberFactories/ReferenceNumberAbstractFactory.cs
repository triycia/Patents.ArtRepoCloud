using System.Text.RegularExpressions;

namespace Patents.ArtRepoCloud.Domain.Factories.ReferenceNumberFactories
{
    public abstract class ReferenceNumberAbstractFactory
    {
        public string Format(string source)
        {
            return source.Replace("-", string.Empty).Replace("/", string.Empty).Replace("\\", string.Empty).Replace(",", string.Empty).Replace("\n", string.Empty).Replace(" ", string.Empty).Trim().ToUpper();
        }

        public bool HasInvalidCharacters(string source)
        {
            return Regex.Match(source, "^[a-zA-Z0-9 ]*$").Success == false;
        }
    }
}