using Patents.ArtRepoCloud.Domain.Enums;

namespace Patents.ArtRepoCloud.Service.ValueObjects.BibData
{
    public class BibDocumentClassification
    {
        public BibDocumentClassification(string text, ClassificationType type)
        {
            Text = text;
            Type = type;
        }
        public string Text { get; }
        public ClassificationType Type { get; }
    }
}