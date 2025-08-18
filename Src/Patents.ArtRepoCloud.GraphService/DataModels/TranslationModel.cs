namespace Patents.ArtRepoCloud.GraphService.DataModels
{
    public class TranslationModel
    {
        public TranslationModel(
            bool isOriginalLanguage,
            bool isDefault,
            string language,
            string title,
            string? @abstract,
            string? description,
            IEnumerable<ClaimModel> claims,
            Guid guid)
        {
            IsOriginalLanguage = isOriginalLanguage;
            IsDefault = isDefault;
            Language = language;
            Title = title;
            Abstract = @abstract;
            Description = description;

            Claims = claims;
            Guid = guid;
        }

        public bool IsOriginalLanguage { get; }
        public bool IsDefault { get; }
        public string Language { get; }
        public string Title { get; }
        public string? Abstract { get; }
        public string? Description { get; }
        public IEnumerable<ClaimModel> Claims { get; }
        public Guid Guid { get; }
    }
}