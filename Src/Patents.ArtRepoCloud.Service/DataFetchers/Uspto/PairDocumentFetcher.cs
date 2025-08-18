using Patents.ArtRepoCloud.Domain.Code.Interfaces;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchRequests;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses;
using Patents.ArtRepoCloud.Service.DataProviders.Uspto.Interfaces;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Service.Code.Attributes;
using Patents.ArtRepoCloud.Service.Enums;
using Patents.ArtRepoCloud.Service.ValueObjects.PairData;
using Patents.ArtRepoCloud.Service.DataProviders.Uspto.ValueObject;
using Vikcher.Framework.Reflection;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.DataProviders.Uspto.Contracts.AttorneyAgents;
using Patents.ArtRepoCloud.Service.Exceptions;
using Patents.ArtRepoCloud.Service.Extensions;

namespace Patents.ArtRepoCloud.Service.DataFetchers.Uspto
{
    [ImportSource(ImportSource.Uspto)]
    public class PairDocumentFetcher : IFetcher<PairDocumentFetchRequest, PairDocumentFetchResponse>
    {
        private readonly IUsptoApiProxy _apiProxy;
        private readonly IReferenceNumberParser _referenceNumberParser;
        private readonly ILogger<PairDocumentFetcher> _logger;

        public PairDocumentFetcher(IUsptoApiProxy apiProxy, IReferenceNumberParser referenceNumberParser, ILogger<PairDocumentFetcher> logger)
        {
            _apiProxy = apiProxy;
            _referenceNumberParser = referenceNumberParser;
            _logger = logger;
        }

        public async Task<PairDocumentFetchResponse> ProcessAsync(PairDocumentFetchRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var searchNumber = request.ReferenceNumber.ToPairNumber();

                var appResult = await _apiProxy.GetApplicationData(searchNumber, cancellationToken)
                    .ConfigureAwait(false);

                if (!appResult.Success)
                {
                    return Failed(HttpReasonCode.NoData);
                }

                var app = appResult?.Result?.ApplicationMetaData;
                var wipoData = appResult?.Result?.WipoApplicationData;

                if (app == null && wipoData?.RelatedApplicationNumberText != null)
                {
                    searchNumber = new PairNumber(wipoData.RelatedApplicationNumberText,
                        UsptoSearchNumberType.ApplicationNumber);

                    appResult = await _apiProxy.GetApplicationData(searchNumber, cancellationToken)
                        .ConfigureAwait(false);

                    app = appResult?.Result?.ApplicationMetaData;
                }

                if (app == null)
                {
                    return Failed(HttpReasonCode.NoData);
                }

                var examiner = appResult?.Result?.ExaminerDetails;
                var inventor = appResult?.Result?.Inventors?.FirstOrDefault();
                var pubData = appResult?.Result?.EarliestPublicationDataList?.FirstOrDefault();

                var appDataDto = new ApplicationDataDto(
                    app.ApplicationIdentification.ApplicationNumberText?.Trim() ?? string.Empty,
                    app.ApplicationIdentification.PublicationNumber?.Trim(),
                    app.InventionTitle,
                    app.FilingDate,
                    app.EffectiveFilingDate,
                    appResult?.Result?.EarliestPublicationDataList?.MinBy(x => x.PublicationDate)?.PublicationDate,
                    app.GrantDate,
                    FormatApplicationType(app.InventionSubjectMatterCategory, app.ApplicationTypeCategory, app.ApplicationIdentification.ApplicationNumberText?.Trim() ?? string.Empty),
                    examiner != null ? $"{examiner.FamilyName}, {examiner.GivenName} {examiner.MiddleName}" : string.Empty,
                    app.GroupArtUnitNumber,
                    app.ApplicationConfirmationNumber,
                    app.DocketNumber,
                    app.Class,
                    app.Subclass,
                    inventor != null
                        ? $"{inventor.FirstName} {inventor.LastName}, {inventor.CityName}, {inventor.GeographicRegionName}"
                        : null,
                    app.CustomerNumber,
                    app.ApplicationStatusDescriptionText,
                    app.ApplicationStatusDate,
                    pubData != null
                        ? $"{pubData.IpOfficeCode} {pubData.PublicationYear}-{pubData.PublicationSequenceNumber} {pubData.PatentDocumentKindCode}"
                        : null,
                    pubData?.PublicationDate,
                    app.PatentNumber,
                    app.GrantDate);

                var continuityDto = await LoadContinuity(searchNumber, cancellationToken).ConfigureAwait(false);
                var transactionsDto = await LoadTransactions(searchNumber, cancellationToken).ConfigureAwait(false);
                var attorneyAgentsDto = await LoadAttorneyAgents(searchNumber, cancellationToken).ConfigureAwait(false);

                return new PairDocumentFetchResponse(
                    appDataDto,
                    continuityDto,
                    transactionsDto,
                    attorneyAgentsDto,
                    request.ReferenceNumber,
                    HttpReasonCode.Success,
                    ImportSource.Uspto);
            }
            catch (ImportProcessFailureReasonException exp)
            {
                _logger.LogInformation(exp, $"{exp.ImportSource.GetName()} document import failed for \"{request.ReferenceNumber}\". Reason: ({exp.ReasonCode}){exp.ReasonCode.GetName()}.");

                return Failed(exp.ReasonCode);
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, $"Failed to retrieve the PAIR document data for \"{request.ReferenceNumber}\".");

                return Failed(HttpReasonCode.Failed);
            }

            PairDocumentFetchResponse Failed(HttpReasonCode code) => new(request.ReferenceNumber, code, ImportSource.Uspto);
        }

        async Task<ContinuityDto?> LoadContinuity(PairNumber pairNumber, CancellationToken cancellationToken)
        {
            var continuityResult = await _apiProxy.GetContinuity(pairNumber, cancellationToken).ConfigureAwait(false);

            if (continuityResult.Success)
            {
                var childContinuities = continuityResult.Result?.ChildContinuities.Select((c, i) =>
                    new ContinuityDto.Continuity(
                        c.ParentApplicationNumberText,
                        c.ChildApplicationNumberText,
                        UsptoDictionaries.ContinuityApplicationStatus.GetValueOrDefault<int, string>(
                            c.ApplicationStatusCategory, string.Empty),
                        UsptoDictionaries.ContinuityDescription.GetValueOrDefault<string, string>(
                            c.ClaimTypeCategory ?? string.Empty, string.Empty),
                        c.FilingDate,
                        i
                    )) ?? Enumerable.Empty<ContinuityDto.Continuity>();

                var parentContinuities = continuityResult.Result?.ParentContinuities.Select((c, i) =>
                    new ContinuityDto.Continuity(
                        c.ParentApplicationNumberText,
                        c.ChildApplicationNumberText,
                        UsptoDictionaries.ContinuityApplicationStatus.GetValueOrDefault<int, string>(
                            c.ApplicationStatusCategory, string.Empty),
                        UsptoDictionaries.ContinuityDescription.GetValueOrDefault<string, string>(
                            c.ClaimTypeCategory ?? string.Empty, string.Empty),
                        c.FilingDate,
                        i
                    )) ?? Enumerable.Empty<ContinuityDto.Continuity>();

                return new ContinuityDto(parentContinuities.ToList(), childContinuities.ToList());
            }

            return null;
        }

        async Task<TransactionsDto> LoadTransactions(PairNumber pairNumber, CancellationToken cancellationToken)
        {
            var transactionsResult = await _apiProxy.GetTransactionHistory(pairNumber, cancellationToken)
                .ConfigureAwait(false);

            var transactions = transactionsResult.Result?.TransactionHistoryList
                                   .OrderBy(t => t.RecordedDate)
                                   .Select((t, i) => new TransactionHistoryDto(t.RecordedDate, t.CaseActionDescriptionText, i))
                               ?? Enumerable.Empty<TransactionHistoryDto>();

            return new TransactionsDto(transactions);
        }

        async Task<AttorneyAgentsDto> LoadAttorneyAgents(PairNumber pairNumber, CancellationToken cancellationToken)
        {
            var result = await _apiProxy.GetAttorneyAgents(pairNumber, cancellationToken)
                .ConfigureAwait(false);

            if (result.Success)
            {
                var correspondence = result.Result.AttorneyAgentsDictionary.GetValueOrDefault("correspondenceAddress");
                var powerOfAttorney = result.Result.AttorneyAgentsDictionary.GetValueOrDefault("powerOfAttorneyAddress");

                return new AttorneyAgentsDto(
                    FillAddressContract(correspondence),
                    FillAddressContract(powerOfAttorney));
            }

            throw new PairActionFailureException(
                PairActionType.AddressAttorneyAgents,
                result.ReasonCode,
                result.Message);
        }

        private AddressContractDto? FillAddressContract(AddressContract? addressContract)
        {
            return addressContract != null
                   && addressContract.CustomerNumber != null
                   && addressContract.CustomerAddress != null
                ? new AddressContractDto(
                    addressContract.CustomerNumber,
                    FillHelper.Fill<AddressDto>(addressContract.CustomerAddress),
                    addressContract.CustomerAttorneys?
                        .Select(x => FillHelper.Fill<CustomerAttorneyDto>(x)) ?? Enumerable.Empty<CustomerAttorneyDto>())
                : null;
        }

        string FormatApplicationType(string inventionSubjectMatterCategory, string applicationTypeCategory, string applicationNumberText)
        {
            string t = inventionSubjectMatterCategory,
                e = applicationTypeCategory,
                appNumber = applicationNumberText;

            string Regular(string t) => "UTL" == t ? "Utility" : "DES" == t ? "Design" : "PLT" == t ? "Plant" : string.Empty;

            return "PROVSNL" == e ? "Provisional" :
                "REGULAR" == e ? Regular(t) :
                "PCT" == e ? "PCT" :
                "REEXAM" != e || "90" != ApplicationNumberFunc(appNumber).Substring(0, 2) &&
                "95" != ApplicationNumberFunc(appNumber).Substring(0, 2) ? "REISSUE" == e ? "Re-Issue" :
                "REEXAM" == e && "96" == ApplicationNumberFunc(appNumber).Substring(0, 2) ? "Supplemental Examination" :
                string.Empty : "Re-Examination";
        }

        string StrSplice(string str, int i, int step, string separator) => str.Substring(0, i) + (separator ?? "") + str.Substring(i + step);

        string? GetUnFormattedAppNo(string? str) =>
            str != null
                ? str.Trim().Length != 8
                    ? str.Trim()
                    : StrSplice(StrSplice(str.Trim(), 5, 0, ","), 2, 0, "/")
                : null;

        string ApplicationNumberFunc(string appNumber) => !string.IsNullOrEmpty(appNumber) ? GetUnFormattedAppNo(appNumber) ?? string.Empty : string.Empty;
    }
}