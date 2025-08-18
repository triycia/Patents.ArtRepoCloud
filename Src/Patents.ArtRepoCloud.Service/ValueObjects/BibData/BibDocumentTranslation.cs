namespace Patents.ArtRepoCloud.Service.ValueObjects.BibData
{
    public class BibDocumentTranslation
    {
        public BibDocumentTranslation(
            string language, 
            bool isOriginalLanguage, 
            string title, 
            string @abstract, 
            string description, 
            IEnumerable<BibDocumentTranslationClaim> claims, 
            bool isDefault = false)
        {
            Language = language;
            IsOriginalLanguage = isOriginalLanguage;
            Title = title;
            Abstract = @abstract;
            Description = description;
            Claims = claims;
            IsDefault = isDefault;
        }


        public string Language { get; }
        public bool IsOriginalLanguage { get; }
        public string Title { get; }
        public string Abstract { get; }
        public string Description { get; }
        public IEnumerable<BibDocumentTranslationClaim> Claims { get; }
        public string ClaimText
        {
            get
            {
                return Claims == null ? null : string.Join(@"\n", Claims.OrderBy(y => y.Number).Select(c => c.Text));
            }
        }
        public bool IsDefault { get; }
    }
}