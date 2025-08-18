using Patents.ArtRepoCloud.Domain.Factories.Interfaces;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using MediatR;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.ParseReferenceNumbers
{
    public class ParseReferenceNumbersCommandHandler : IRequestHandler<ParseReferenceNumbersCommand, ParseReferenceNumbersCommandResult>
    {
        private readonly IReferenceNumberFactory[] _factories;

        public ParseReferenceNumbersCommandHandler(IReferenceNumberFactory[] factories)
        {
            _factories = factories;
        }

        public Task<ParseReferenceNumbersCommandResult> Handle(ParseReferenceNumbersCommand query, CancellationToken cancellationToken)
        {
            return Task.FromResult(new ParseReferenceNumbersCommandResult(query.ReferenceNumbers.Select(Parse)));
        }

        private ParsedReferenceNumber Parse(string referenceNumber)
        {
            foreach (var referenceNumberFactory in _factories)
            {
                var refNumber = referenceNumberFactory.Parse(referenceNumber);

                if (refNumber != null)
                {
                    return new ParsedReferenceNumber(
                        referenceNumber,
                        refNumber.CountryCode,
                        refNumber.Number,
                        refNumber.KindCode,
                        refNumber.NumberType);
                }
            }

            return new ParsedReferenceNumber(referenceNumber);
        }
    }
}