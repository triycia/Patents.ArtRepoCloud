using Patents.ArtRepoCloud.Domain.Enums;

namespace Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate
{
    public class DocumentClassification
    {
        private DocumentClassification() {}

        public DocumentClassification(string classification, ClassificationType classificationType)
        {
            Classification = classification;
            ClassificationType = classificationType;
        }

        public string Classification { get; private set; }
        public ClassificationType ClassificationType { get; private set; }
    }
}