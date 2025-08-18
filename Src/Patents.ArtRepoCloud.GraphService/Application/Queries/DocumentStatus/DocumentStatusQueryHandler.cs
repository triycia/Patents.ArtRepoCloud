using Patents.ArtRepoCloud.Domain.Code.Interfaces;
using Patents.ArtRepoCloud.Domain.Enums;
using Vikcher.Framework.Common.CustomizedExceptions;
using MediatR;
using Newtonsoft.Json;
using Patents.ArtRepoCloud.GraphService.Configuration;
using Patents.ArtRepoCloud.GraphService.DataModels;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Patents.ArtRepoCloud.GraphService.Application.Queries.DocumentStatus
{
    public class DocumentStatusQueryHandler : IRequestHandler<DocumentStatusQuery, DocumentStatusQueryResult>
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IReferenceNumberParser _referenceNumberParser;
        private readonly IDocumentRepository _patentRepository;
        private readonly IpdIfiApiSettings _apiSettings;
        private readonly ILogger<DocumentStatusQueryHandler> _logger;

        public DocumentStatusQueryHandler(
            IHttpClientFactory clientFactory,
            IReferenceNumberParser referenceNumberParser,
            IDocumentRepository patentRepository,
            IpdIfiApiSettings apiSettings,
            ILogger<DocumentStatusQueryHandler> logger)
        {
            _clientFactory = clientFactory;
            _referenceNumberParser = referenceNumberParser;
            _patentRepository = patentRepository;
            _apiSettings = apiSettings;
            _logger = logger;
        }

        public async Task<DocumentStatusQueryResult> Handle(DocumentStatusQuery query, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Started handling {nameof(DocumentStatusQuery)} for {query.ReferenceNumber}.");

            var referenceNumber = _referenceNumberParser.Parse(query.ReferenceNumber);

            if (referenceNumber == null)
            {
                _logger.LogWarning($"Unable to parse ReferenceNumber {query.ReferenceNumber}");

                return new DocumentStatusQueryResult(query.ReferenceNumber);
            }
            
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiSettings.DocumentStatusUrl}?countryCode={referenceNumber.CountryCode}&number={referenceNumber.Number}&kindCode={referenceNumber.KindCode}");

            var client = _clientFactory.CreateClient("IPD.IFI.ApiService");

            var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var jsonData = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogDebug($"Successfully completed {nameof(DocumentStatusQuery)} for {query.ReferenceNumber}.");

            var result = JsonConvert.DeserializeObject<IpdIfiDocumentStatusResultModel>(jsonData);

            var data = result?.DocumentStatus;

            return new DocumentStatusQueryResult(
                query.ReferenceNumber,
                data?.PatentStatus ?? string.Empty,
                data?.PatentLastLegalEvent ?? string.Empty,
                data?.PatentLastLegalEventDate,
                data?.PteStatus ?? string.Empty,
                data?.ProductName ?? string.Empty,
                data?.ExtensionRequested ?? string.Empty,
                data?.PteStatusDate,
                data?.ExpiryDate);
        }
    }
}