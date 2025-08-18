using Patents.ArtRepoCloud.Domain.Code.Interfaces;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.Factories.Interfaces;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using System.Text.RegularExpressions;

namespace Patents.ArtRepoCloud.Domain.Code
{
    public class ReferenceNumberParser : IReferenceNumberParser
    {
        private readonly IReferenceNumberFactory[] _referenceNumberFactories;

        public ReferenceNumberParser(IReferenceNumberFactory[] referenceNumberFactories)
        {
            _referenceNumberFactories = referenceNumberFactories;
        }

        public ReferenceNumber? Parse(string? identifier)
        {
            if (string.IsNullOrEmpty(identifier)) { return null; }

            foreach (var referenceNumberFactory in _referenceNumberFactories)
            {
                var refNumber = referenceNumberFactory.Parse(identifier);

                if (refNumber != null)
                {
                    return refNumber;
                }
            }

            return null;
        }
    }
}