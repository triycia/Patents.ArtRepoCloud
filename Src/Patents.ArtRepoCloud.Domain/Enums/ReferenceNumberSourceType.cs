using Patents.ArtRepoCloud.Domain.Attributes;

namespace Patents.ArtRepoCloud.Domain.Enums
{
    public enum ReferenceNumberSourceType
    {
        Unknown = 0,

        [DocumentType(DocumentType.UsPatent)]
        UsPatent = 1,

        [DocumentType(DocumentType.UsApplication)]
        UsApplication = 2,

        [DocumentType(DocumentType.WO)]
        Wipo = 3,

        [DocumentType(DocumentType.EP)]
        Epo = 4,

        MAPatent = 5,

        MANonPatent = 6,

        [DocumentType(DocumentType.SupportDocument)]
        Other = 7
    }
}