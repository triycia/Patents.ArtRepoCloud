using System.Net.Http.Headers;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.Configuration;
using Patents.ArtRepoCloud.Service.DataProviders.Ifi.Contracts;
using Patents.ArtRepoCloud.Service.DataProviders.Ifi.Interfaces;
using Patents.ArtRepoCloud.Service.Enums;
using Patents.ArtRepoCloud.Service.Exceptions;
using Newtonsoft.Json;

namespace Patents.ArtRepoCloud.Service.DataProviders.Ifi
{
    public class IfiApiProxy : IIfiApiProxy
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IficlaimsSettings _settings;
        private readonly ILogger<IfiApiProxy> _logger;

        public IfiApiProxy(IHttpClientFactory clientFactory, IficlaimsSettings settings, ILogger<IfiApiProxy> logger)
        {
            _clientFactory = clientFactory;
            _settings = settings;
            _logger = logger;
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            var client = _clientFactory.CreateClient("IficlaimsService");

            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            return response;
        }

        public async Task<string> GetDocumentAsync(string ucid, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, _settings.BibDocumentUrl(ucid)), cancellationToken).ConfigureAwait(false);

            await ValidateResult(response, cancellationToken);

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        public async Task<string?> GetUcidAsync(IReferenceNumber referenceNumber, CancellationToken cancellationToken)
        {
            var url = _settings.UcidUrl(referenceNumber.SourceReferenceNumber, referenceNumber.CountryCode);

            HttpResponseMessage response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, url), cancellationToken).ConfigureAwait(false);

            await ValidateResult(response, cancellationToken);

            var jsonData = await response.Content.ReadAsStringAsync(cancellationToken);

            var content = JsonConvert.DeserializeObject<IfiResult>(jsonData)?.Content;

            if (content?.Response?.Docs == null || content.Response.NumFound < 1)
            {
                _logger.LogInformation($"No ifi records found for {referenceNumber}.");

                return null;
            }

            if (content.Response.NumFound == 1)
            {
                var result = content.Response.Docs.First().Ucid;

                if (!string.IsNullOrWhiteSpace(result))
                {
                    _logger.LogInformation($"Successfully retrieved ucid {result} for request {referenceNumber}.");

                    return result;
                }
            }

            if (content.Response.NumFound > 1)
            {
                var result = (content.Response.Docs.FirstOrDefault(d => d.Ucid == referenceNumber.ToString()) ?? content.Response.Docs.First(d => d.Ucid.Contains("-" + referenceNumber.Number + "-"))).Ucid;

                if (!string.IsNullOrWhiteSpace(result))
                {
                    _logger.LogInformation($"Successfully retrieved ucid {result} for request {referenceNumber}.");

                    return result;
                }
            }

            _logger.LogInformation($"Failed retrieved ucid for request {referenceNumber}.");

            return null;
        }

        public async Task<IList<IfiAttachment>> AttachmentListAsync(string ucid, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, _settings.AttachmentListUrl(ucid)), cancellationToken).ConfigureAwait(false);

            await ValidateResult(response, cancellationToken);

            var jsonData = await response.Content.ReadAsStringAsync(cancellationToken);

            return (JsonConvert.DeserializeObject<IfiResult>(jsonData)?.Attachments ?? Enumerable.Empty<IfiAttachment>()).ToList();
        }

        public async Task<Stream> AttachmentFetchAsync(string ifiPath, CancellationToken cancellationToken)
        {
            var response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, _settings.AttachmentFetchUrl(ifiPath)), cancellationToken).ConfigureAwait(false);

            await ValidateResult(response, cancellationToken);

            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }

        public async Task<Stream> AttachmentFetchAllAsync(string ucid, CancellationToken cancellationToken)
        {
            var response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, _settings.AttachmentFetchAllUrl(ucid)), cancellationToken).ConfigureAwait(false);

            await ValidateResult(response, cancellationToken);

            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }

        public async Task<IList<string>> GetFamilySimpleAsync(int familyId, CancellationToken cancellationToken)
        {
            var response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, _settings.FamilySimpleUrl(familyId)), cancellationToken).ConfigureAwait(false);

            await ValidateResult(response, cancellationToken);

            var simpleFamilyDataStr = await response.Content.ReadAsStringAsync(cancellationToken);

            var simpleFamilyResult = JsonConvert.DeserializeObject<IfiResult>(simpleFamilyDataStr);

            return (simpleFamilyResult?.Content.Response.Docs.Select(r => r.Ucid) ?? Enumerable.Empty<string>()).ToList();
        }

        public async Task<IList<string>> GetFamilyExtendedAsync(string ucid, CancellationToken cancellationToken)
        {
            var response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, _settings.FamilyExtendedUrl(ucid)), cancellationToken).ConfigureAwait(false);

            await ValidateResult(response, cancellationToken);

            var extendedFamilyDataStr = await response.Content.ReadAsStringAsync(cancellationToken);

            var extendedFamilyResult = JsonConvert.DeserializeObject<IfiFamilyMembers>(extendedFamilyDataStr);

            return (extendedFamilyResult?.FamilyMembers.Members ?? Enumerable.Empty<string>()).ToList();
        }

        private async Task ValidateResult(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (!response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync(cancellationToken);

                if (data.Trim().EndsWith("</html>", StringComparison.CurrentCultureIgnoreCase) && data.Contains("Please come back later"))
                {
                    throw new ImportProcessFailureReasonException(ImportSource.Ifi, HttpReasonCode.FetchSourceUnavailable, "Ifi provider is currently not available!");
                }
            }
        }
    }
}