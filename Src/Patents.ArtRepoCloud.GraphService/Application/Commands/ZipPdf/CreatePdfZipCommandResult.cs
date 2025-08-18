namespace Patents.ArtRepoCloud.GraphService.Application.Commands.ZipPdf
{
    public class CreatePdfZipCommandResult
    {
        public CreatePdfZipCommandResult(string identifier)
        {
            Identifier = identifier;
        }

        public string Identifier { get; }
    }
}