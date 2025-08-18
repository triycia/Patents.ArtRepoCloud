using Patents.ArtRepoCloud.Domain.Enums;

namespace Patents.ArtRepoCloud.Domain.Attributes
{
    public class DocumentTypeAttribute : Attribute
    {
        public DocumentType DocumentType { get; }

        public DocumentTypeAttribute(DocumentType documentType)
        {
            DocumentType = documentType;
        }
    }
}