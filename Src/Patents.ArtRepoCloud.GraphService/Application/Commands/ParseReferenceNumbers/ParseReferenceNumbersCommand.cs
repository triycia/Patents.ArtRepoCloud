using MediatR;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.ParseReferenceNumbers
{
    public class ParseReferenceNumbersCommand : IRequest<ParseReferenceNumbersCommandResult>
    {
        public ParseReferenceNumbersCommand(IEnumerable<string> referenceNumbers)
        {
            ReferenceNumbers = referenceNumbers;
        }

        public IEnumerable<string> ReferenceNumbers { get; }
    }
}