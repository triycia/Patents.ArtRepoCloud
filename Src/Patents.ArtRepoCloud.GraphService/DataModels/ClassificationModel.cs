using Patents.ArtRepoCloud.Domain.Enums;

namespace Patents.ArtRepoCloud.GraphService.DataModels
{
    public class ClassificationModel
    {
        public ClassificationModel(string classification, ClassificationType classificationType)
        {
            Classification = classification;
            ClassificationType = classificationType;
        }

        public string Classification { get; }
        public ClassificationType ClassificationType { get; }
    }
}