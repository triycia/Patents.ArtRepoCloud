using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Vikcher.Framework.Common;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Service.DataFetchers.ValueObjects;
using Patents.ArtRepoCloud.Service.DataProviders.Ifi.Contracts;
using Patents.ArtRepoCloud.Service.Enums;
using Patents.ArtRepoCloud.Service.Exceptions;
using Patents.ArtRepoCloud.Service.Extensions;
using Patents.ArtRepoCloud.Service.ValueObjects.BibData;

namespace Patents.ArtRepoCloud.Service.DataFetchers
{
    internal class IfiXDocument
    {
        public static XDocument? _document;
        private static XElement? Root => _document?.Root;
        private static ReferenceNumberSourceType _referenceNumberSourceType;

        public IfiXDocument() : base()
        {
        }

        public IfiXDocument(XDocument other)
        {
            _document = other;
        }

        private ReferenceMetadata? _publicationReference;
        public IReferenceMetadata PublicationReference => _publicationReference ?? ParsePubReferenceData();
        
        private ReferenceMetadata? _applicationReference;
        public IReferenceMetadata ApplicationReference => _applicationReference ?? ParseApplicationReferenceData();

        private DateTime? _earliestPriorityDate;
        public DateTime? EarliestPriorityDate => _earliestPriorityDate ?? ParseEarliestPriorityDate();

        private string? _earliestPriorityNumber;
        public string? EarliestPriorityNumber => _earliestPriorityNumber ?? ParseEarliestPriorityNumber();

        private string? _assigneeName;
        public string? AssigneeName => _assigneeName ?? ParseAssignee();

        private string? _authors;
        public string? Authors => _authors ?? ParseAuthors();

        private DateTime? _expirationDate;
        public DateTime? ExpirationDate => _expirationDate ?? ParseExpirationDate();

        private DateTime? _originalExpirationDate;
        public DateTime? OriginalExpirationDate => _originalExpirationDate ?? ParseOriginalExpirationDate();

        private DateTime? _adjustedExpirationDate;
        public DateTime? AdjustedExpirationDate => _adjustedExpirationDate ?? ParseAdjustedExpirationDate();

        private int? _termAdjustment;
        public int TermAdjustment => _termAdjustment ?? ParseTermAdjustment();

        private int? _familyId;
        public int FamilyId => _familyId ?? ParseFamilyId();

        private string? _terminalDisclaimer;
        public string? TerminalDisclaimer => _terminalDisclaimer ?? ParseTerminalDisclaimer();

        private IEnumerable<BibDocumentTranslation>? _translations;
        public IEnumerable<BibDocumentTranslation> Translations => _translations ?? ParseTranslations();

        private IList<BibDocumentClassification>? _classifications;
        public IEnumerable<BibDocumentClassification> Classifications => _classifications ?? ParseClasses();

        private IEnumerable<IfiImageXmlItem>? _images;
        public IEnumerable<IfiImageXmlItem> Images => _images ?? ParseImages();

        public static async Task<IfiXDocument?> LoadAsync(string xmlData, LoadOptions options, ReferenceNumberSourceType referenceNumberSourceType, CancellationToken cancellationToken)
        {
            TextReader textReader = new StringReader(xmlData);

            _document = await XDocument.LoadAsync(textReader, options, cancellationToken).ConfigureAwait(false);

            if (_document.Root?.HasElements ?? false)
            {
                _referenceNumberSourceType = referenceNumberSourceType;

                return new IfiXDocument(_document);
            }

            if ((_document.Root?.HasAttributes ?? false))
            {
                var ucid = _document.Root.Attribute("ucid")?.Value;

                if (!string.IsNullOrEmpty(ucid))
                {
                    throw new ImportProcessFailureReasonException(ImportSource.Ifi, HttpReasonCode.NoData, $"Ifi has No Data for ucid: {ucid}");
                }
            }

            throw new ImportProcessFailureReasonException(ImportSource.Ifi, HttpReasonCode.NoData);
        }

        public IReferenceMetadata ParsePubReferenceData()
        {
            var elem = Root?.Descendants("publication-reference").FirstOrDefault();

            if (elem != null)
            {
                _publicationReference = new ReferenceMetadata();

                _publicationReference.SetLanguage(elem.Descendants("lang").FirstOrDefault()?.Value);
                _publicationReference.SetCountry(elem.Descendants("country").FirstOrDefault()?.Value);
                _publicationReference.SetDate(elem.Descendants("date").Select(n => (DateTime?)DateTime.ParseExact(n.Value, "yyyyMMdd", CultureInfo.InvariantCulture)).FirstOrDefault());
            }

            return _publicationReference;
        }

        public IReferenceMetadata ParseApplicationReferenceData()
        {
            var elem = Root?.Descendants("application-reference").FirstOrDefault();

            if (elem != null)
            {
                var countryElements = elem.Descendants("country").ToList();

                _applicationReference = new ReferenceMetadata();

                if (countryElements.Any())
                {
                    if (countryElements.First().Value.ToUpper() == "US")
                    {
                        var ucidAttribute = elem.Attribute("ucid");
                        var appRefInfo = ucidAttribute?.Value.Split('-');

                        _applicationReference.SetCountry(appRefInfo?[0]);
                        _applicationReference.SetNumber(appRefInfo?.Any() ?? false ? appRefInfo?[1] : null);
                    }
                    else
                    {
                        _applicationReference.SetCountry(countryElements.Select(n => n.Value.ToUpper()).FirstOrDefault());
                        _applicationReference.SetNumber(elem.Descendants("doc-number").Select(n => n.Value).FirstOrDefault());
                    }
                }

                _applicationReference.SetSeries(elem.Attribute("us-series-code")?.Value);
                _applicationReference.SetDate(elem.Descendants("date").Select(n => (DateTime?)DateTime.ParseExact(n.Value, "yyyyMMdd", CultureInfo.InvariantCulture)).FirstOrDefault());

                if (!string.IsNullOrEmpty(_applicationReference.Series))
                {
                    var length = _applicationReference.Number?.Length ?? -1;

                    var number = length is <= 8 and > -1 ? _applicationReference.Number?.PadLeft(8, '0').Substring(0, 6) : _applicationReference.Number?.Substring(length - 6, 6);

                    _applicationReference.SetNumber(number);
                }

            }

            return _applicationReference;
        }

        #region Priority Data

        private XElement? earliestPriorityClaim;

        private void ParseClaimsData()
        {
            var priorityClaims = Root?.Descendants("priority-claims").SingleOrDefault();

            if (priorityClaims != null)
            {
                if (priorityClaims.Elements().Any(n => n.Descendants("date").Any()))
                {
                    earliestPriorityClaim = priorityClaims.Elements().OrderBy(n => n.Descendants("date").Single().Value).First().Elements("document-id").FirstOrDefault();
                }
            }
        }

        public string ParseEarliestPriorityNumber()
        {
            return earliestPriorityClaim != null 
                ? _earliestPriorityNumber = $"{earliestPriorityClaim?.Element("country")?.Value}{earliestPriorityClaim?.Element("doc-number")?.Value}"
                : string.Empty;
        }

        public DateTime? ParseEarliestPriorityDate()
        {
            return _earliestPriorityDate = earliestPriorityClaim?.Elements("date").Select(n => (DateTime?)DateTime.ParseExact(n.Value, "yyyyMMdd", CultureInfo.InvariantCulture)).FirstOrDefault();
        }

        #endregion

        #region Parties
        private string? _applicantFormat;
        private string ParseApplicantFormat()
        {
            Guard.AssertNotNull(Root, nameof(Root));
            var format = "original";

            if (Root.Descendants("applicant").Any(n => (string)n.Attribute("format") == "intermediate"))
            {
                format = "intermediate";
            }

            return format;
        }

        public string? ParseAssignee()
        {
            _applicantFormat ??= ParseApplicantFormat();

            Guard.AssertNotNull(Root, nameof(Root));
            Guard.AssertNotNullOrEmptyOrWhiteSpace(_applicantFormat, nameof(_applicantFormat));

            _assigneeName = (Root?.Descendants("assignee").Descendants("addressbook").Descendants("name").Select(x => x.Value).FirstOrDefault() ??
                          Root?.Descendants("assignee").Descendants("addressbook").Descendants("last-name").Select(x => x.Value).FirstOrDefault() ??
                          Root?.Descendants("applicant").Where(x => (string)x.Attribute("format") == "epo").Descendants("addressbook").Descendants("last-name").Select(x => x.Value).FirstOrDefault() ??
                          Root?.Descendants("applicant").Where(x => (string)x.Attribute("format") == "intermediate").Descendants("addressbook").Descendants("last-name").Select(x => x.Value).FirstOrDefault() ??
                          Root?.Descendants("applicant").Where(x => (string)x.Attribute("format") == "original").Descendants("addressbook").Descendants("last-name").Select(x => x.Value).FirstOrDefault()) ??
                          Root?.Descendants("applicant").Where(n => (string)n.Attribute("format") == _applicantFormat && (string)n.Attribute("sequence") == "1").Descendants("last-name").Select(n => n.Value).FirstOrDefault();
            return _assigneeName;
        }

        public string ParseAuthors()
        {
            _applicantFormat ??= ParseApplicantFormat();

            Guard.AssertNotNull(Root, nameof(Root));
            Guard.AssertNotNullOrEmptyOrWhiteSpace(_applicantFormat, nameof(_applicantFormat));

            _authors = string.Join("; ", Root.Descendants("inventor").Where(n => (string)n.Attribute("format") == _applicantFormat).Select(n =>
            {
                var firstName = n.Descendants("first-name").FirstOrDefault()?.Value;
                var lastName = n.Descendants("last-name").FirstOrDefault()?.Value;
                return $"{firstName} {lastName}".RemoveRedundantSpaces();
            }));

            return _authors;
        }

#endregion

        #region Parse Classes

        public IList<BibDocumentClassification> ParseClasses()
        {
            ParseIpcClasses();
            ParseUsClasses();

            return _classifications!;
        }
        private void ParseIpcClasses()
        {
            Guard.AssertNotNull(Root, nameof(Root));

            _classifications ??= new List<BibDocumentClassification>();

            string ipcClassRegex = @".*?([a-h]\s*\d{2}\s*[a-z])\s*(\d{1,3}\s*/\s*\d{2,}).*?";

            var classifications = Root?.Descendants("classification-ipcr").Select(n => n.Value.RemoveRedundantSpaces()).Where(n => !string.IsNullOrEmpty(n));

            foreach (var classification in classifications)
            {
                var text = classification.Trim();
                var type = ClassificationType.IpcClass;

                var match = Regex.Match(text, ipcClassRegex, RegexOptions.IgnoreCase);
                text = match.Success ? $"{match.Groups[1].Value.Replace(" ", "")} {match.Groups[2].Value.Replace(" ", "")}" : text;

                if (!_classifications.Any(c => c.Text == text && c.Type == type))
                {
                    _classifications.Add(new BibDocumentClassification(text, type));
                }
            }
        }

        private void ParseUsClasses()
        {
            Guard.AssertNotNull(Root, nameof(Root));

            _classifications ??= new List<BibDocumentClassification>();

            var classifications = Root.Descendants("technical-data").Descendants("classification-national").Where(n => (string)n.Attribute("country") == "US").Elements().Select(n => n.Value).Distinct().Select(n => $"{n.PadLeft(3, '0').Substring(0, 3)}/{(n.Length < 7 ? n.PadLeft(3, '0').Substring(3).PadLeft(3, '0') : n.Substring(3, 3))}{(n.Length > 6 ? "." + n.Substring(6) : "")}");

            foreach (var classification in classifications)
            {
                var text = classification.Trim();
                var type = ClassificationType.UsClass;

                if (!_classifications.Any(c => c.Text == text && c.Type == type))
                {
                    _classifications.Add(new BibDocumentClassification(text, type));
                }
            }
        }

        #endregion

        #region Parse Statuses

        private XElement? _statusNode;

        public XElement? ParsePatentStatusElement()
        {
            Guard.AssertNotNull(Root, nameof(Root));
            return Root.Descendants("ifi-patent-status").FirstOrDefault();
        }

        public DateTime? ParseExpirationDate()
        {
            _statusNode ??= ParsePatentStatusElement();

            var adjustedValue = _statusNode?.Attribute("adjusted-expiration")?.Value;

            if (adjustedValue != null)
            {
                _expirationDate = DateTime.ParseExact(adjustedValue, "yyyyMMdd", CultureInfo.InvariantCulture);
            }

            if (_expirationDate != null)
                return _expirationDate;

            var anticipatedValue = _statusNode?.Attribute("anticipated-expiration")?.Value;

            if (anticipatedValue != null)
            {
                _expirationDate = DateTime.ParseExact(anticipatedValue, "yyyyMMdd", CultureInfo.InvariantCulture);
            }

            return _expirationDate;
        }

        public DateTime? ParseOriginalExpirationDate()
        {
            _statusNode ??= ParsePatentStatusElement();

            var value = _statusNode?.Attribute("anticipated-expiration")?.Value;

            return _originalExpirationDate = value != null ? DateTime.ParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture) : _originalExpirationDate;
        }

        public int ParseTermAdjustment()
        {
            _statusNode ??= ParsePatentStatusElement();

            var extensionValue = _statusNode?.Descendants("ifi-term-extension").FirstOrDefault()?.Value;

            return (int)(_termAdjustment = int.TryParse(extensionValue, out int value) ? value : -1);
        }

        public DateTime? ParseAdjustedExpirationDate()
        {
            _statusNode ??= ParsePatentStatusElement();

            var attributeValue = _statusNode?.Attribute("adjusted-expiration")?.Value;

            return _adjustedExpirationDate = !string.IsNullOrWhiteSpace(attributeValue)
                ? (DateTime?)DateTime.ParseExact(attributeValue, "yyyyMMdd", CultureInfo.InvariantCulture)
                : null;
        }

        public string ParseTerminalDisclaimer()
        {
            _statusNode ??= ParsePatentStatusElement();

            _terminalDisclaimer = _statusNode?.Attribute("terminal-disclaimer")?.Value;

            return _terminalDisclaimer;
        }

        #endregion

        public int ParseFamilyId()
        {
            Guard.AssertNotNull(Root, nameof(Root));

            if (Root.Attribute("family-id") == null)
                return -1;

            var familyId = Root.Attribute("family-id")?.Value;

            if (!int.TryParse(familyId, out int value))
                return -1;

            _familyId = value;

            return value;
        }

        private IEnumerable<BibDocumentTranslation> ParseTranslations()
        {
            var languages = Root
                .Descendants("technical-data")
                .Descendants("invention-title")
                .Select(n => ((string)n.Attribute("lang")).ToUpper())
                .Distinct()
                .ToList();

            return languages.Select(lang => CreateTranslation(lang, languages));
        }

        public string ParseAbstractText(string lang)
        {
            Guard.AssertNotNull(Root, nameof(Root));
            Guard.AssertNotNullOrEmptyOrWhiteSpace(lang, nameof(lang));
            var abstractText = RemoveXmlComment(FindProperString(Root.Descendants("abstract").Where(n => (string)n.Attribute("lang") == lang)));

            if (!string.IsNullOrEmpty(abstractText))
                abstractText = Regex.Replace(abstractText.Replace("</p>", Environment.NewLine), "<.*?>", string.Empty);

            return abstractText ?? string.Empty;
        }

        private string BuildTranslationDescription(string lang)
        {
            var description = RemoveXmlComment(FindProperString(Root.Descendants("description").Where(n => (string)n.Attribute("lang") == lang)));

            if (description != null)
            {
                if (_referenceNumberSourceType == ReferenceNumberSourceType.UsApplication)
                {
                    description = Regex.Replace(description, @"<\s*p[^>]*\s+num\s*=\s*(?:\'|\"")\D*(\d+)(?:\'|\"")[^>]*>", "[$1]", RegexOptions.IgnoreCase);
                    description = Regex.Replace(description, @"\[(\d{3,5})]\s*\[\1]", "[$1]");
                }

                description = Regex.Replace(description.Replace("</p>", Environment.NewLine), "<.*?>", string.Empty);
            }

            return description;
        }

        public IList<BibDocumentTranslationClaim> ParseClaims(string lang)
        {
            Guard.AssertNotNull(Root, nameof(Root));
            Guard.AssertNotNullOrEmptyOrWhiteSpace(lang, nameof(lang));

            var claimsElement = FindAcceptableElement(Root.Descendants("claims").Where(n => (string)n.Attribute("lang") == lang));
            var claimList = new List<BibDocumentTranslationClaim>();

            if (claimsElement != null)
            {
                var claimElements = claimsElement.Descendants("claim");

                if (claimElements.Any())
                {
                    if (claimElements.Count() > 1)
                    {
                        claimList.AddRange(claimElements.SelectMany(c => ParseClaim(c) ?? new BibDocumentTranslationClaim[] { }).Where(e => e != null));
                    }
                    else
                    {
                        var index = 1;
                        var claimTextElements = claimElements.FirstOrDefault().Descendants("claim-text");
                        claimList.AddRange(claimTextElements.SelectMany(claimElement =>
                            ParseClaim(claimElement, ref index) ?? new BibDocumentTranslationClaim[] { }).Where(e => e != null));
                    }
                }
            }

            return claimList;
        }

        public IEnumerable<BibDocumentTranslationClaim> ParseClaim(XElement claimElement, ref int index)
        {
            Guard.AssertNotNull(Root, nameof(Root));
            Guard.AssertNotNull(claimElement, nameof(claimElement));
            int numIndex;

            return claimElement.Attribute("num") != null
                ? ParseClaim(claimElement)
                : new[]
                {
                    new BibDocumentTranslationClaim
                    (
                        numIndex = index++,
                        ParseClaimText(claimElement),
                        ParseClaimIsIndependent(ParseClaimXAttributeByNum(numIndex)) ?? ParseClaimIsIndependent(claimElement),
                        ParseClaimIsCanceled(ParseClaimXAttributeByNum(numIndex))?? ParseClaimIsCanceled(claimElement)
                    )
                };
        }

        public string ParseClaimText(XElement claimElement)
        {
            Guard.AssertNotNull(claimElement, nameof(claimElement));
            return Regex.Replace(RemoveXmlComment(claimElement.InnerXml()).Replace("<claim-text>", Environment.NewLine).Replace("</claim-text>", Environment.NewLine).Replace("</p>", Environment.NewLine), "<[^>]*>", string.Empty);
        }

        public bool? ParseClaimIsIndependent(XAttribute claimAttribute)
        {
            if (claimAttribute == null)
            {
                return null;
            }

            return claimAttribute.Value != "dependent";
        }

        public bool ParseClaimIsIndependent(XElement claimElement)
        {
            Guard.AssertNotNull(claimElement, nameof(claimElement));
            return !claimElement.Descendants("claim-ref").Any();
        }

        public XAttribute ParseIfiClaimType(string expression)
        {
            Guard.AssertNotNull(Root, nameof(Root));
            Guard.AssertNotNullOrEmptyOrWhiteSpace(expression, nameof(expression));
            return ((IEnumerable)Root.XPathEvaluate($@".//ifi-claim[{expression}]/@type")).Cast<XAttribute>().FirstOrDefault();
        }

        public XAttribute ParseClaimXAttributeByRef(string claimRef)
        {
            return ParseIfiClaimType($"@idref={claimRef}");
        }

        public XAttribute ParseClaimXAttributeByNum(int claimNum)
        {
            return ParseIfiClaimType($"@num={claimNum}");
        }

        public IEnumerable<BibDocumentTranslationClaim> ParseClaim(XElement claimElement)
        {
            Guard.AssertNotNull(Root, nameof(Root));
            Guard.AssertNotNull(claimElement, nameof(claimElement));

            int claimNum;

            if (int.TryParse(claimElement.Attribute("num")?.Value, out claimNum))
            {
                return new[]
                {
                    new BibDocumentTranslationClaim
                    (
                        claimNum,
                        ParseClaimText(claimElement),
                        ParseClaimIsIndependent(ParseClaimXAttributeByNum(claimNum)) ?? ParseClaimIsIndependent(claimElement),
                        ParseClaimIsCanceled(ParseClaimXAttributeByNum(claimNum))?? ParseClaimIsCanceled(claimElement)
                    )
                };
            }

            if (!(claimElement.Attribute("num")?.Value.Contains("-") ?? false))
                return null;

            var claimNums = claimElement.Attribute("num")?.Value.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
            int first, second;

            if (claimNums?.Count() != 2 || !int.TryParse(claimNums[0], out first) || !int.TryParse(claimNums[1], out second))
                return null;

            if (second > first)
            {
                return Enumerable.Range(first, (second - first) + 1).Select(i => new BibDocumentTranslationClaim(i, ParseClaimText(claimElement), true, true));
            }

            claimNum = first;

            var claimRef = claimElement.Attribute("id")?.Value;

            return new[]
            {
                new BibDocumentTranslationClaim(claimNum, ParseClaimText(claimElement), ParseClaimIsIndependent(ParseClaimXAttributeByRef(claimRef)) ?? ParseClaimIsIndependent(claimElement), ParseClaimIsCanceled(ParseClaimXAttributeByRef(claimRef))?? ParseClaimIsCanceled(claimElement))
            };
        }

        public bool? ParseClaimIsCanceled(XAttribute claimAttribute)
        {
            if (claimAttribute == null)
            {
                return null;
            }

            return claimAttribute.Value == "canceled";
        }

        public bool ParseClaimIsCanceled(XElement claimElement)
        {
            Guard.AssertNotNull(claimElement, nameof(claimElement));
            return claimElement.InnerXml().ToLower().Contains("canceled");
        }

        private BibDocumentTranslation CreateTranslation(string lang, IEnumerable<string> languages)
        {
            Guard.AssertNotNull(Root, nameof(Root));
            Guard.AssertNotNullOrEmptyOrWhiteSpace(lang, nameof(lang));

            var title = FindProperString(Root.Descendants("invention-title").ToList().Where(x => (string)x.Attribute("lang") == lang));

            var isOriginalLanguage = _publicationReference.Language == lang || (languages.Count() == 1 && lang != "EN");
            var abstractText = ParseAbstractText(lang);
            var description = BuildTranslationDescription(lang);
            var tran = new BibDocumentTranslation(lang, isOriginalLanguage, title, abstractText, description, ParseClaims(lang));

            return tran;
        }

        public XElement FindAcceptableElement(IEnumerable<XElement> nodes)
        {
            if (nodes == null || !nodes.Any())
                return null;

            var nodesList = nodes.ToList();

            return ((nodesList.FirstOrDefault(n => (string)n.Attribute("load-source") == "docdb") ?? nodesList.FirstOrDefault(n => (string)n.Attribute("load-source") == "patent-office")) ?? nodesList.FirstOrDefault(n => (string)n.Attribute("load-source") == "mxw-smt")) ?? nodesList.FirstOrDefault();
        }

        public string FindProperString(IEnumerable<XElement> nodes)
        {
            if (nodes == null || !nodes.Any())
                return string.Empty;

            var stringElement = FindAcceptableElement(nodes);

            return stringElement?.InnerXml();
        }

        public IEnumerable<IfiImageXmlItem> ParseImages()
        {
            Guard.AssertNotNull(Root, nameof(Root));

            var xmlImages = Root.Descendants("img").ToList();
            return xmlImages.GroupBy(x => (x.Attribute("filename") ?? x.Attribute("file"))?.Value)
                .Select(group => group.First())
                .Where(e => e.Parent != null && !string.IsNullOrEmpty(e.Attribute("file")?.Value))
                .Select(e => new IfiImageXmlItem(e.Attribute("file")?.Value!, e.Attribute("orientation")?.Value));
        }

        private string RemoveXmlComment(string input)
        {
            return Regex.Replace(input, "<!--.*?-->", string.Empty);
        }
    }
}