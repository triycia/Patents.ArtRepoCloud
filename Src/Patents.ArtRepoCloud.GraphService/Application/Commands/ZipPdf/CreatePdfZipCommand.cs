using MediatR;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.ZipPdf
{
    public class CreatePdfZipCommand : IRequest<CreatePdfZipCommandResult>
    {
        public CreatePdfZipCommand(IEnumerable<string> referenceNumbers)
        {
            ReferenceNumbers = referenceNumbers;
        }

        public IEnumerable<string> ReferenceNumbers { get; }
    }
}