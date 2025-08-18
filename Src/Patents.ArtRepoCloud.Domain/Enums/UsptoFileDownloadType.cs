using System.Runtime.Serialization;
using Patents.ArtRepoCloud.Domain.Attributes;

namespace Patents.ArtRepoCloud.Domain.Enums
{
    public enum UsptoFileDownloadType
    {
        [MaxPageCount(int.MaxValue)]
        [EnumMember(Value = "pdfzip")]
        PdfZip = 1,

        [MaxPageCount(int.MaxValue)]
        [EnumMember(Value = "pdf")]
        PdfInSingleFile = 2,

        [EnumMember(Value = "")]
        PdfIndividualFile = 3
    }
}