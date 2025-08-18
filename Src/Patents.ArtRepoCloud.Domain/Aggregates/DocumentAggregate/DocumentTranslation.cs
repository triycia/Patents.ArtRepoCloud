using Newtonsoft.Json;

namespace Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate
{
    public class DocumentTranslation
    {
        private DocumentTranslation(){}

        public DocumentTranslation(
            bool isOriginalLanguage,
            bool isDefault,
            string? language, 
            string? title, 
            string? @abstract, 
            string? description, 
            DateTime dateCreated,
            IList<Claim> claims)
        {
            IsOriginalLanguage = isOriginalLanguage;
            IsDefault = isDefault;
            Language = language;
            Title = title;
            Abstract = @abstract;
            Description = description;
            DateCreated = dateCreated;
            Claims = claims;
            Guid = Guid.NewGuid();
        }

        [JsonConstructor]
        public DocumentTranslation(
            bool isOriginalLanguage,
            bool isDefault,
            string? language,
            string? title,
            string? @abstract,
            string? description,
            DateTime dateCreated,
            IList<Claim> claims,
            Guid guid)
        {
            IsOriginalLanguage = isOriginalLanguage;
            IsDefault = isDefault;
            Language = language;
            Title = title;
            Abstract = @abstract;
            Description = description;
            DateCreated = dateCreated;
            Claims = claims;
            Guid = guid;
        }

        public bool IsOriginalLanguage { get; private set; }
        public bool IsDefault { get; private set; }
        public string? Language { get; private set; }
        public string? Title { get; private set; }
        public string? Abstract { get; private set; }
        public string? Description { get; private set; }
        public DateTime DateCreated { get; private set; }

        public IList<Claim> Claims { get; private set; } = new List<Claim>();
        public Guid Guid { get; private set; }

        public void Update(bool isOriginalLanguage, bool isDefault, string? language, string? title, string? @abstract, string? description)
        {
            IsOriginalLanguage = isOriginalLanguage;
            IsDefault = isDefault;
            Language = language;
            Title = title;
            Abstract = @abstract;
            Description = description;
        }

        public void SetIsDefault(bool isDefault)
        {
            IsDefault = isDefault;
        }

        public void AddClaim(Claim claim)
        {
            Claims.Add(claim);
        }

        public void RemoveAllClaims()
        {
            Claims.Clear();
        }
    }
}