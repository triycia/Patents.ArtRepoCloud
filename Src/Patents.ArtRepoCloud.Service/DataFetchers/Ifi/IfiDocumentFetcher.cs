using Patents.ArtRepoCloud.Service.Exceptions;
using Patents.ArtRepoCloud.Service.Enums;
using Patents.ArtRepoCloud.Service.DataProviders.Ifi.Interfaces;
using Patents.ArtRepoCloud.Service.ValueObjects.BibData;
using System.Xml.Linq;
using Patents.ArtRepoCloud.Domain.Code.Interfaces;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.Extensions;
using Patents.ArtRepoCloud.Service.Code.Attributes;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchRequests;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses;
using Patents.ArtRepoCloud.Service.Extensions;
using Patents.ArtRepoCloud.Domain.Factories.Interfaces;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;

namespace Patents.ArtRepoCloud.Service.DataFetchers.Ifi
{
    [ImportSource(ImportSource.Ifi)]
    public class IfiDocumentFetcher : IFetcher<DocumentFetchRequest, DocumentFetchResponse>
    {
        private readonly IIfiApiProxy _apiProxy;
        private readonly IReferenceNumberParser _referenceNumberParser;
        private readonly ILogger<IfiDocumentFetcher> _logger;

        public IfiDocumentFetcher(IIfiApiProxy apiProxy, IReferenceNumberParser referenceNumberParser, ILogger<IfiDocumentFetcher> logger)
        {
            _apiProxy = apiProxy;
            _referenceNumberParser = referenceNumberParser;
            _logger = logger;
        }

        public async Task<DocumentFetchResponse> ProcessAsync(DocumentFetchRequest request, CancellationToken cancellationToken)
        {
            var referenceNumber = request.ReferenceNumber!;

            _logger.LogDebug($"Start processing {nameof(IfiDocumentFetcher)} for reference # {referenceNumber}.");

            try
            {
                var ucid = await _apiProxy.GetUcidAsync(referenceNumber, cancellationToken).ConfigureAwait(false);

                var fromUcidNumber = _referenceNumberParser.Parse(ucid);

                if (!string.IsNullOrEmpty(fromUcidNumber?.KindCode))
                {
                    referenceNumber.SetKindCode(fromUcidNumber.KindCode);
                }

                var xmlData = await _apiProxy.GetDocumentAsync(referenceNumber.Ucid, cancellationToken).ConfigureAwait(false);

                var ifiDoc = await IfiXDocument.LoadAsync(xmlData, LoadOptions.None, referenceNumber.NumberType, cancellationToken).ConfigureAwait(false);

                var doc = new BibDocument(
                    ifiDoc.ApplicationReference?.Number,
                    ifiDoc.PublicationReference?.Country,
                    ifiDoc.TermAdjustment,
                    ifiDoc.FamilyId,
                    ifiDoc.EarliestPriorityDate,
                    ifiDoc.ApplicationReference?.Date,
                    ifiDoc.PublicationReference.Date,
                    ifiDoc.PublicationReference?.Date,
                    ifiDoc.ExpirationDate,
                    ifiDoc.OriginalExpirationDate,
                    ifiDoc.AdjustedExpirationDate,
                    ifiDoc.TerminalDisclaimer,
                    ifiDoc.EarliestPriorityNumber,
                    ifiDoc.AssigneeName);

                if (ifiDoc.Translations.Any())
                {
                    ifiDoc.Translations.ToList().ForEach(doc.AddTranslation);
                }

                if (ifiDoc.Classifications.Any())
                {
                    ifiDoc.Classifications.ToList().ForEach(doc.AddClassification);
                }

                if (ifiDoc.FamilyId > -1)
                {
                    var simpleFamily = await _apiProxy.GetFamilySimpleAsync(ifiDoc.FamilyId, cancellationToken).ConfigureAwait(false);

                    if (simpleFamily.Any())
                    {
                        simpleFamily.Distinct().ToList().ForEach(doc.AddFamilySimple);
                    }
                }

                var extendedFamilyList = await _apiProxy.GetFamilyExtendedAsync(referenceNumber.Ucid, cancellationToken).ConfigureAwait(false);

                if (extendedFamilyList.Any())
                {
                    extendedFamilyList.Distinct().ToList().ForEach(doc.AddFamilyExtended);
                }

                _logger.LogInformation($"Successfully retrieved the bib document for request \"{referenceNumber}\".");

                return new DocumentFetchResponse(referenceNumber, doc, HttpReasonCode.Success, ImportSource.Ifi);
            }
            catch (ImportProcessFailureReasonException exp)
            {
                _logger.LogInformation($"{exp.ImportSource.GetName()} document import failed for \"{request.ReferenceNumber}\". Reason: ({exp.ReasonCode}){exp.ReasonCode.GetName()}.");

                return Failed(exp.ReasonCode);
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, $"Failed to retrieve the document for \"{request.ReferenceNumber}\".");

                return Failed(HttpReasonCode.Failed);
            }

            DocumentFetchResponse Failed(HttpReasonCode code) => new(request.ReferenceNumber, code, ImportSource.Uspto);
        }
    }
}