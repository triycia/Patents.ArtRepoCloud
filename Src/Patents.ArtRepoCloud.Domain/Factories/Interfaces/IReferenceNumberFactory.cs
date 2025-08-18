using Patents.ArtRepoCloud.Domain.ReferenceNumbers;

namespace Patents.ArtRepoCloud.Domain.Factories.Interfaces
{
    public interface IReferenceNumberFactory
    {
        ReferenceNumber? Parse(string referenceNumber);
    }
}