using Patents.ArtRepoCloud.Service.DataProviders.IpdIfi.Contracts;
using Patents.ArtRepoCloud.Service.DataProviders.IpdIfi.Interfaces;
using Patents.ArtRepoCloud.Service.Enums;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Service.Code.Attributes;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchRequests;
using Patents.ArtRepoCloud.Domain.Code.Interfaces;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.Exceptions;
using Patents.ArtRepoCloud.Service.Extensions;

namespace Patents.ArtRepoCloud.Service.DataFetchers.IpdIfi
{
    [ImportSource(ImportSource.Ifi)]
    internal class IpdIfiDocumentFetcher : IFetcher<DocumentFetchRequest, DocumentFetchResponse>
    {
        private readonly IIpdIfiApiProxy _apiProxy;
        private readonly IReferenceNumberParser _referenceNumberParser;
        private readonly ILogger<IpdIfiDocumentFetcher> _logger;

        public IpdIfiDocumentFetcher(IIpdIfiApiProxy apiProxy, IReferenceNumberParser referenceNumberParser, ILogger<IpdIfiDocumentFetcher> logger)
        {
            _apiProxy = apiProxy;
            _referenceNumberParser = referenceNumberParser;
            _logger = logger;
        }

        public async Task<DocumentFetchResponse> ProcessAsync(DocumentFetchRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Started handling document retrieval request for {request.ReferenceNumber}.");

            var referenceNumber = request.ReferenceNumber;

            try
            {
                var result = await _apiProxy.GetDocumentAsync(request.ReferenceNumber, cancellationToken).ConfigureAwait(false);

                if (result?.Status == IpdIfiRequestStatus.Success)
                {
                    _logger.LogInformation($"{nameof(IpdIfiDocumentFetcher)} succeeded for reference #: {request.ReferenceNumber}.");

                    var docNum = result.Document!.DocumentNumber;

                    if (result.Document!.DocumentNumber!.ToString() != request.ReferenceNumber.ToString())
                    {
                        referenceNumber = new ReferenceNumber(
                            docNum.CountryCode, 
                            docNum.Number, 
                            docNum.KindCode,
                            referenceNumber.SeparatorFormat,
                            referenceNumber.SourceReferenceNumber, 
                            referenceNumber.NumberType);
                    }

                    return new DocumentFetchResponse(referenceNumber, result.Document, HttpReasonCode.Success, ImportSource.Ifi);
                }

                return Failed(HttpReasonCode.NoData);
            }
            catch (ImportProcessFailureReasonException exp)
            {
                _logger.LogInformation($"{exp.ImportSource.GetName()} document import failed for \"{request.ReferenceNumber}\". Reason: ({exp.ReasonCode}){exp.ReasonCode.GetName()}.");

                return Failed(exp.ReasonCode);
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, $"{nameof(IpdIfiDocumentFetcher)} Request Failed for reference #: {referenceNumber}.");

                return Failed(HttpReasonCode.Failed);
            }

            DocumentFetchResponse Failed(HttpReasonCode code) => new(request.ReferenceNumber, code, ImportSource.Ifi);
        }
    }
}