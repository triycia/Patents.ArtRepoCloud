using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.Configuration;
using Vikcher.Framework.Common;
using Patents.ArtRepoCloud.Service.Enums;
using Patents.ArtRepoCloud.Service.DataProviders.Epo.Interfaces;
using System.Text.RegularExpressions;
using Patents.ArtRepoCloud.Service.ValueObjects.BibData;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Service.Code.Attributes;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchRequests;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses;
using Patents.ArtRepoCloud.Service.Exceptions;
using Patents.ArtRepoCloud.Service.Extensions;

namespace Patents.ArtRepoCloud.Service.DataFetchers.Epo
{
    [ImportSource(ImportSource.Epo)]
    internal class EpoDocumentFetcher : IFetcher<DocumentFetchRequest, DocumentFetchResponse>
    {
        private readonly IEpoApiProxy _apiProxy;
        private readonly EpoSettings _settings;
        private readonly ILogger<EpoDocumentFetcher> _logger;

        public EpoDocumentFetcher(IEpoApiProxy apiProxy, EpoSettings settings, ILogger<EpoDocumentFetcher> logger)
        {
            _apiProxy = apiProxy;
            _settings = settings;
            _logger = logger;
        }

        public async Task<DocumentFetchResponse> ProcessAsync(DocumentFetchRequest request, CancellationToken cancellationToken)
        {

            _logger.LogDebug($"Started processing {nameof(EpoDocumentFetcher)} document retrieval request for {request.ReferenceNumber}.");

            try
            {
                var referenceNumber = request.ReferenceNumber;

                var epoDoc = await _apiProxy.GetBibDocumentAsync(referenceNumber, cancellationToken).ConfigureAwait(false);

                var descriptionData = await _apiProxy.GetDescriptionAsync(referenceNumber, cancellationToken).ConfigureAwait(false);

                var descriptionTxt = ParseTranslationDescription(referenceNumber, descriptionData);

                var claimsData = await _apiProxy.GetClaimsAsync(referenceNumber, cancellationToken).ConfigureAwait(false);

                var claimsList = ParseClaims(claimsData);
                var claims = claimsList.Select(claimItem =>
                {
                    var number = claimItem.Item1;
                    var text = claimItem.Item2;
                    // TODO: these isIndependent and isCanceled are not defined in the v2 implementation. We are defaulting these to false for now.
                    const bool isIndependent = false;
                    const bool isCanceled = false;
                    return new BibDocumentTranslationClaim(number, text, isIndependent, isCanceled);
                }).ToList();


                var publicationData = epoDoc.PublicationReference;

                var applicationData = epoDoc.ApplicationReference;

                var assignees = epoDoc.ParseAssigneeName();

                var translation = BuildTranslation(referenceNumber, epoDoc.AbstractText, epoDoc.InventionTitle, descriptionTxt, claims);

                var classificationIpcr = epoDoc.ParseClassificationIpcr().Distinct().ToList(); ;

                var doc = new BibDocument(
                    applicationData.Number,
                    publicationData.Country,
                    null,
                    null,
                    null,
                    applicationData.Date,
                    publicationData.Date,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    assignees);

                if (classificationIpcr.Any())
                {
                    classificationIpcr.ForEach(text => doc.AddClassification(new BibDocumentClassification(text, ClassificationType.IpcClass)));
                }

                doc.AddTranslation(translation);

                _logger.LogDebug($"Epo data fetched document {referenceNumber.SourceReferenceNumber}");

                return new DocumentFetchResponse(referenceNumber, doc, HttpReasonCode.Success, ImportSource.Epo);
            }
            catch (ImportProcessFailureReasonException exp)
            {
                _logger.LogInformation($"{exp.ImportSource.GetName()} document import failed for \"{request.ReferenceNumber}\". Reason: ({exp.ReasonCode}){exp.ReasonCode.GetName()}.");

                return Failed(exp.ReasonCode);
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, $"{nameof(EpoDocumentFetcher)} request failed for \"{request.ReferenceNumber}\".");

                return Failed(HttpReasonCode.Failed);
            }

            DocumentFetchResponse Failed(HttpReasonCode code) => new(request.ReferenceNumber, code, ImportSource.Epo);
        }

        private BibDocumentTranslation BuildTranslation(ReferenceNumber referenceNumber, string abstractText, string inventionTitle, string description, List<BibDocumentTranslationClaim> claims)
        {
            Guard.AssertNotNull(referenceNumber, nameof(referenceNumber));


            var translation = new BibDocumentTranslation("EN", true, inventionTitle, abstractText, description, claims);

            return translation;
        }

        string? ParseTranslationDescription(ReferenceNumber referenceNumber, string fullTextResponse)
        {
            Guard.AssertNotNull(referenceNumber, nameof(referenceNumber));

            if (string.IsNullOrEmpty(fullTextResponse))
                return null;

            var result = fullTextResponse.Replace("<p>", Environment.NewLine);
            result = result.Replace("</p>", Environment.NewLine);
            result = Regex.Replace(result, "<.*?>", string.Empty);
            result = result.Replace(referenceNumber.ToString(), string.Empty);
            result = result.Replace("Description", string.Empty);
            return result.Trim();
        }

        IEnumerable<Tuple<int, string>> ParseClaims(string claimsResponse)
        {
            if (string.IsNullOrEmpty(claimsResponse))
                return new List<Tuple<int, string>>();

            var claimsText = claimsResponse.Replace("<claim-text>", Environment.NewLine);
            claimsText = claimsText.Replace("</claim-text>", Environment.NewLine);
            claimsText = Regex.Replace(claimsText, "<.*?>", string.Empty);
            claimsText = claimsText.Replace("\r", string.Empty);
            return BuildClaimsDictionary(claimsText, "\n");
        }
        
        IEnumerable<Tuple<int, string>> BuildClaimsDictionary(string claimsText, string claimsTextSeparator)
        {
            var claimsList = claimsText.Split(new[] { claimsTextSeparator }, StringSplitOptions.None).Where(item => item.Trim().Length > 0);

            var claimNumbersList = new List<int>();
            var claimTextList = new List<string>();

            foreach (var claim in claimsList)
            {
                var claimMatch = Regex.Match(claim.Trim(), @"^(?<ClaimNumber>\d*\.|\[.*\d*\]) (?<ClaimText>.+)$", RegexOptions.Singleline);

                if (claimMatch.Success)
                {
                    var claimNumberMatch = new Regex(@"[^\d]");
                    var input = claimNumberMatch.Replace(claimMatch.Groups["ClaimNumber"].Value.Trim(), string.Empty);
                    var claimNumber = ParseInt(input);

                    if (claimNumber.HasValue)
                    {
                        var claimText = claimMatch.Groups["ClaimText"].Value.Trim();
                        claimNumbersList.Add(claimNumber.Value);
                        claimTextList.Add(claimText);
                    }
                }
            }

            return claimNumbersList.Select((claimNumber, index) => new Tuple<int, string>(index + 1, claimTextList[index]));
        }

        int? ParseInt(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }
            return int.TryParse(input, out var value) ? (int?)value : null;
        }
    }
}