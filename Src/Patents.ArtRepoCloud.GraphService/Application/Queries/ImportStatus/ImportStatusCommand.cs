using MediatR;

namespace Patents.ArtRepoCloud.GraphService.Application.Queries.ImportStatus
{
    public class ImportStatusCommand : IRequest<ImportStatusCommandResult>
    {
        public ImportStatusCommand(IEnumerable<string> referenceNumbers)
        {
            ReferenceNumbers = referenceNumbers;
        }

        public IEnumerable<string> ReferenceNumbers { get; }
    }
}