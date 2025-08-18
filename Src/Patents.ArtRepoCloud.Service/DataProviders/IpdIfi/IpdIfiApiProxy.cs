using Patents.ArtRepoCloud.Service.Configuration;
using Patents.ArtRepoCloud.Service.DataProviders.IpdIfi.Interfaces;
using System.Net.Http.Headers;
using System.Net;
using Newtonsoft.Json;
using Patents.ArtRepoCloud.Service.DataProviders.IpdIfi.Contracts;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;

namespace Patents.ArtRepoCloud.Service.DataProviders.IpdIfi
{
    public class IpdIfiApiProxy : IIpdIfiApiProxy
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IpdIfiApiSettings _settings;

        public IpdIfiApiProxy(IHttpClientFactory clientFactory, IpdIfiApiSettings settings)
        {
            _clientFactory = clientFactory;
            _settings = settings;
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            var client = _clientFactory.CreateClient("IPD.IFI.ApiService");

            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode.Equals(HttpStatusCode.Unauthorized))
            {

            }

            response.EnsureSuccessStatusCode();

            return response;
        }

        public async Task<IpdIfiDocumentResult?> GetDocumentAsync(ReferenceNumber referenceNumber, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_settings.DocumentUrl}?countryCode={referenceNumber.CountryCode}&number={referenceNumber.Number}&kindCode={referenceNumber.KindCode}");

            HttpResponseMessage response = await SendAsync(request, cancellationToken).ConfigureAwait(false);

            var jsonData = await response.Content.ReadAsStringAsync(cancellationToken);

            return JsonConvert.DeserializeObject<IpdIfiDocumentResult>(jsonData);
        }

        public async Task<IpdIfiImageAttachmentsResult?> GetImageAttachmentsAsync(ReferenceNumber referenceNumber, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"{_settings.ImageAttachmentsUrl}?countryCode={referenceNumber.CountryCode}&number={referenceNumber.Number}&kindCode={referenceNumber.KindCode}");

            HttpResponseMessage response = await SendAsync(request, cancellationToken).ConfigureAwait(false);

            var jsonData = await response.Content.ReadAsStringAsync(cancellationToken);

            return JsonConvert.DeserializeObject<IpdIfiImageAttachmentsResult>(jsonData);
        }
    }
}