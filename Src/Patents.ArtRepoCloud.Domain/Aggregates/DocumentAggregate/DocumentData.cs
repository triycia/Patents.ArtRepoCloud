using Patents.ArtRepoCloud.Domain.Extensions;
using Newtonsoft.Json;

namespace Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate
{
    public class DocumentData
    {
        private DocumentData(){}

        public DocumentData(string? terminalDisclaimer, string? assigneeName, string? authors)
        {
            TerminalDisclaimer = terminalDisclaimer;
            AssigneeName = assigneeName;
            Authors = authors;
        }

        [JsonConstructor]
        public DocumentData(
            string? terminalDisclaimer, 
            string? assigneeName, 
            string? authors,
            IList<DocumentClassification> documentClassifications,
            IList<DocumentTranslation> documentTranslations,
            string? userUrl = null, 
            string? userNotes = null)
        {
            TerminalDisclaimer = terminalDisclaimer;
            AssigneeName = assigneeName;
            Authors = authors;
            DocumentClassifications = documentClassifications;
            DocumentTranslations = documentTranslations;
            UserUrl = userUrl;
            UserNotes = userNotes;
        }

        public string? TerminalDisclaimer { get; private set; }
        public string? AssigneeName { get; private set; }
        public string? Authors { get; private set; }
        public IList<DocumentClassification> DocumentClassifications { get; private set; } = new List<DocumentClassification>();
        public IList<DocumentTranslation> DocumentTranslations { get; private set; } = new List<DocumentTranslation>();
        public string? UserUrl { get; }
        public string? UserNotes { get; }
        public DocumentTranslation? DefaultTranslation => DocumentTranslations.FirstOrDefault(dt => dt.IsDefault) 
                                                          ?? DocumentTranslations.FirstOrDefault(dt => !string.IsNullOrWhiteSpace(dt.Language) 
                                                              && dt.Language.Equals("EN", StringComparison.CurrentCultureIgnoreCase))
                                                          ?? DocumentTranslations.FirstOrDefault(dt => dt.IsOriginalLanguage);

        public void SetTerminalDisclaimer(string? value)
        {
            TerminalDisclaimer = value;
        }

        public void SetAssigneeName(string? assigneeName)
        {
            AssigneeName = assigneeName;
        }

        public void SetAuthors(string? authors)
        {
            Authors = authors;
        }

        public void AddDocumentTranslation(DocumentTranslation translation)
        {
            DocumentTranslations.Add(translation);
        }
    }
}