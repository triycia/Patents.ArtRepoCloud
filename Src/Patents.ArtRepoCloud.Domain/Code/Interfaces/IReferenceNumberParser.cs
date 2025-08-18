using Patents.ArtRepoCloud.Domain.ReferenceNumbers;

namespace Patents.ArtRepoCloud.Domain.Code.Interfaces
{
    public interface IReferenceNumberParser
    {
        ReferenceNumber? Parse(string? identifier);
    }
}