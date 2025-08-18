using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using MediatR;

namespace Patents.ArtRepoCloud.Service.Application.Commands.EnqueueFamily
{
    public class EnqueueFamilyCommand : IRequest<EnqueueFamilyCommandResult>
    {
        public EnqueueFamilyCommand(ReferenceNumber referenceNumber, IEnumerable<string> familyReferenceNumbers)
        {
            ReferenceNumber = referenceNumber;
            FamilyReferenceNumbers = familyReferenceNumbers;
        }

        public ReferenceNumber ReferenceNumber { get; }
        public IEnumerable<string> FamilyReferenceNumbers { get; }
    }
}