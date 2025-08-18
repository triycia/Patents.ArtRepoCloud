using System.Globalization;
using System.Xml.Linq;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.DataFetchers.ValueObjects;

namespace Patents.ArtRepoCloud.Service.DataFetchers
{
    public class EpoXDocument
    {
        public static XDocument? _document;
        private static XElement? Root => _document?.Root?.Descendants().FirstOrDefault(x => x.Name.LocalName == "exchange-document");
        private static ReferenceNumberSourceType _referenceNumberSourceType;

        public EpoXDocument()
        {
        }

        public EpoXDocument(XDocument other)
        {
            _document = other;
        }

        private ReferenceMetadata? _publicationReference;
        public IReferenceMetadata PublicationReference => _publicationReference ?? ParsePubReferenceData();

        private ReferenceMetadata? _applicationReference;
        public IReferenceMetadata ApplicationReference => _applicationReference ?? ParseApplicationReferenceData();

        private string? _assigneeName;
        public string AssigneeName => _assigneeName ?? ParseAssigneeName();

        private string? _abstractText;
        public string AbstractText => _abstractText ?? ParseAbstractText();

        private string? _inventionTitle;
        public string InventionTitle => _inventionTitle ?? ParseInventionTitle();

        private IEnumerable<string>? _classification;
        public IEnumerable<string> ClassificationIpcr => _classification ?? ParseClassificationIpcr();

        public static async Task<EpoXDocument> LoadAsync(string xmlData, LoadOptions options, ReferenceNumberSourceType referenceNumberSourceType, CancellationToken cancellationToken)
        {
            TextReader textReader = new StringReader(xmlData);

            _document = await XDocument.LoadAsync(textReader, options, cancellationToken).ConfigureAwait(false);
            _referenceNumberSourceType = referenceNumberSourceType;

            return new EpoXDocument(_document);
        }

        public IReferenceMetadata ParsePubReferenceData()
        {
            var elem = Root?.Descendants().FirstOrDefault(x => x.Name.LocalName == "publication-reference");
            var elem2 = Root?.Descendants("publication-reference").FirstOrDefault();

            if (elem != null)
            {
                _publicationReference = new ReferenceMetadata();

                _publicationReference.SetCountry(elem.Descendants().FirstOrDefault(x => x.Name.LocalName == "country")?.Value);
                _publicationReference.SetDate(elem.Descendants().Where(x => x.Name.LocalName == "date").Select(n => (DateTime?)DateTime.ParseExact(n.Value, "yyyyMMdd", CultureInfo.InvariantCulture)).FirstOrDefault());
            }

            return _publicationReference;
        }

        public IReferenceMetadata ParseApplicationReferenceData()
        {
            var elem = Root?.Descendants().FirstOrDefault(x => x.Name.LocalName == "application-reference");

            if (elem != null)
            {
                _applicationReference = new ReferenceMetadata();

                var docNumber = elem.Descendants().FirstOrDefault(x => x.Name.LocalName == "document-id" && x.Attribute("document-id-type")?.Value == "original")?.Descendants().FirstOrDefault(x => x.Name.LocalName == "doc-number")?.Value;
                var date = elem.Descendants().Where(x => x.Name.LocalName == "date").Select(n => (DateTime?)DateTime.ParseExact(n.Value, "yyyyMMdd", CultureInfo.InvariantCulture)).FirstOrDefault();

                _applicationReference.SetCountry(elem.Descendants().FirstOrDefault(x => x.Name.LocalName == "country")?.Value);
                _applicationReference.SetDate(date);
                _applicationReference.SetNumber(docNumber);
            }

            return _applicationReference;
        }

        public string ParseAssigneeName()
        {
            var names = Root?.Descendants().Where(x => x.Name.LocalName == "applicant")
                .Select(x => x.Descendants().FirstOrDefault(x => x.Name.LocalName == "name")?.Value.Trim())
                .Where(name => !string.IsNullOrEmpty(name));

            _assigneeName = names != null 
                ? string.Join(";", names) 
                : string.Empty;

            return _assigneeName;
        }

        public string ParseAbstractText()
        {
            var abstractTextList = Root?.Descendants()
                .Where(x => x.Name.LocalName == "abstract" && x.Attribute("lang")?.Value == "en")
                .Descendants().Where(x => x.Name.LocalName == "p")
                .Select(x => x.Value)
                .ToList();

            _abstractText = abstractTextList.Any() ? string.Join(" ", abstractTextList) : string.Empty;

            return _abstractText;
        }

        public string ParseInventionTitle()
        {
            _inventionTitle = Root?.Descendants().FirstOrDefault(x => x.Name.LocalName == "invention-title" && x.Attribute("lang")?.Value == "en")?.Value ?? string.Empty;

            return _inventionTitle;
        }

        public IEnumerable<string> ParseClassificationIpcr()
        {
            _classification = Root?
                .Descendants().Where(x => x.Name.LocalName == "classification-ipcr")
                .Descendants().Where(x => x.Name.LocalName == "text")
                .Select(x => x.Value) ?? Enumerable.Empty<string>();

            return _classification;
        }

        private static string GetFormattedReferenceNumber(ReferenceNumber referenceNumber)
        {
            return referenceNumber.CountryCode.Equals("EP", StringComparison.CurrentCultureIgnoreCase) || referenceNumber.CountryCode.Equals("WO") ? (referenceNumber.KindCode.Equals(string.Empty) ? referenceNumber.SourceReferenceNumber : referenceNumber.SeparatorFormat) : referenceNumber.ToString();
        }
    }
}