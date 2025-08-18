using Patents.ArtRepoCloud.Service.Configuration;
using Patents.ArtRepoCloud.Service.DataProviders.Ifi;
using Patents.ArtRepoCloud.Service.DataProviders.Uspto.Contracts;
using Patents.ArtRepoCloud.Service.DataProviders.Uspto.Interfaces;
using Patents.ArtRepoCloud.Service.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Patents.ArtRepoCloud.Service.Extensions;
using Autofac;
using Patents.ArtRepoCloud.Service.DataProviders.Uspto.RequestFactory;
using Patents.ArtRepoCloud.Service.DataProviders.Uspto.ValueObject;
using Patents.ArtRepoCloud.Service.Factories.HttpFactory;
using Patents.ArtRepoCloud.Service.Factories.HttpFactory.Interfaces;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Vikcher.Framework.Common;
using Microsoft.Azure.Amqp;
using System.Net;
using Newtonsoft.Json;
using Patents.ArtRepoCloud.Service.ValueObjects;
using Patents.ArtRepoCloud.Service.ValueObjects.PairData;
using Patents.ArtRepoCloud.Service.Exceptions;
using Patents.ArtRepoCloud.Service.DataProviders.Uspto.Contracts.AttorneyAgents;

namespace Patents.ArtRepoCloud.Service.DataProviders.Uspto
{
    public class UsptoApiProxy : IUsptoApiProxy
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly UsptoApiSettings _settings;
        private readonly ILifetimeScope _lifetimeScope;
        private readonly ILogger<UsptoApiProxy> _logger;

        private static string _token = string.Empty;

        #region MyRegion

        public UsptoApiProxy(IHttpClientFactory clientFactory, UsptoApiSettings settings, ILifetimeScope lifetimeScope, ILogger<UsptoApiProxy> logger)
        {
            _clientFactory = clientFactory;
            _settings = settings;
            _lifetimeScope = lifetimeScope;
            _logger = logger;
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            var client = _clientFactory.CreateClient("PatentCenter");
            //client.BaseAddress = new Uri($"http://{_settings.IPAddress}");

            var tag = _lifetimeScope.Tag;

           

            if (!string.IsNullOrEmpty(_token))
            {
                requestMessage.Headers.Add("X-AUTH-TOKEN", _token);
            }

            try
            {
                var response = await client.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

                ValidateToken(response.Headers);

                return response;
            }
            catch (Exception exp)
            {
                throw;
            }
        }

        void InitializeHeaders(HttpRequestMessage requestMessage, string searchNumber, string? referrerId = null)
        {
            requestMessage.Headers.Add("Referer", $"https://patentcenter.uspto.gov/applications/{searchNumber}/ifw/docs");
            requestMessage.Headers.Add("ReferrerId", referrerId ?? UsptoHelper.InitReferrerId());
            requestMessage.Headers.Add("Host", "patentcenter.uspto.gov");
            requestMessage.Headers.Add("Origin", "https://patentcenter.uspto.gov");
            requestMessage.Headers.Add("sec-ch-ua", "'.Not/A)Brand';v='99', 'Google Chrome';v='103', 'Chromium';v='103'");
            requestMessage.Headers.Add("sec-ch-ua-mobile", "?0");
            requestMessage.Headers.Add("sec-ch-ua-platform", "Windows");
            requestMessage.Headers.Add("Sec-Fetch-Dest", "empty");
            requestMessage.Headers.Add("Sec-Fetch-Mode", "cors");
            requestMessage.Headers.Add("Sec-Fetch-Site", "same-origin");
            requestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36");
            requestMessage.Headers.Add("Accept-Language", "uk-UA,uk;q=0.9,en-US;q=0.8,en;q=0.7");
            requestMessage.Headers.Add("Connection", "keep-alive");
        }

        public async Task<IRequestResult<ApplicationDataContract?>> GetApplicationData(PairNumber pairNumber, CancellationToken cancellationToken)
        {
            var url = $"{_settings.ApplicationDataUrl}?{UrlQueryParameter.GetValueOrDefault(pairNumber.Type, UrlQueryParameter[UsptoSearchNumberType.ApplicationNumber])}={pairNumber.Number}";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            InitializeHeaders(requestMessage, pairNumber.Number);

            var response = await SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorMsg = await response.ResponseAsAsync<ApplicationErrorMessage>(cancellationToken).ConfigureAwait(false);

                return new RequestResult<ApplicationDataContract>(
                    errorMsg.ReasonCode,
                    false,
                    errorMsg.Result?.Errors.Any() ?? false
                        ? errorMsg.Result?.Errors.FirstOrDefault() ?? errorMsg.Message
                        : errorMsg.Message);
            }
            else if (!response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync(cancellationToken);

                var isNotFound = jsonStr?.Contains("404 Not Found") ?? false;
                var reason = isNotFound ? HttpReasonCode.NotFound : response.StatusCode.ToReasonCode();
                var message = isNotFound ? "404 Not Found" : reason.GetName();

                return new RequestResult<ApplicationDataContract>(reason, false, message!);
            }

            return await response.ResultAsAsync<ApplicationDataContract>(cancellationToken);
        }

        public async Task<IRequestResult<ContinuityDataContract?>> GetContinuity(PairNumber pairNumber, CancellationToken cancellationToken)
        {
            var url = string.Format(_settings.ContinuityUrl, pairNumber.Number);
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            InitializeHeaders(requestMessage, pairNumber.Number);

            var response = await SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new RequestResult<ContinuityDataContract>(HttpReasonCode.NotFound, false, "404 Not Found");
            }
            else if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.ResponseAsAsync<ErrorMessage>(cancellationToken).ConfigureAwait(false);
                var isNotFound = errorMsg.Result?.Message?.Contains("404 Not Found") ?? false;
                var reason = isNotFound ? HttpReasonCode.NotFound : response.StatusCode.ToReasonCode();
                var message = isNotFound ? "404 Not Found" : errorMsg.Result?.Error ?? reason.GetName();

                return new RequestResult<ContinuityDataContract>(reason, false, message!);
            }

            return await response.ResultAsAsync<ContinuityDataContract>(cancellationToken);
        }

        public async Task<IRequestResult<TransactionHistoryDataContract>> GetTransactionHistory(PairNumber pairNumber, CancellationToken cancellationToken)
        {
            var url = string.Format(_settings.TransactionHistoryUrl, pairNumber.Number);
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            InitializeHeaders(requestMessage, pairNumber.Number);

            var response = await SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new RequestResult<TransactionHistoryDataContract>(HttpReasonCode.NotFound, false, "404 Not Found");
            }
            else if (!response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync(cancellationToken);

                var isNotFound = jsonStr?.Contains("404 Not Found") ?? false;
                var reason = isNotFound ? HttpReasonCode.NotFound : response.StatusCode.ToReasonCode();
                var message = isNotFound ? "404 Not Found" : reason.GetName();

                return new RequestResult<TransactionHistoryDataContract>(reason, false, message ?? string.Empty);
            }

            return await response.ResultAsAsync<TransactionHistoryDataContract>(cancellationToken);
        }

        public async Task<IRequestResult<AttorneyAgentsContract?>> GetAttorneyAgents(PairNumber pairNumber, CancellationToken cancellationToken)
        {
            var url = string.Format(_settings.AddressesUrl, pairNumber.Number);
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            InitializeHeaders(requestMessage, pairNumber.Number);

            var response = await SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new PairActionFailureException(
                    PairActionType.AddressAttorneyAgents,
                    HttpReasonCode.NotFound,
                    "404 Not Found");
            }
            else if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.ResponseAsAsync<ErrorMessage>(cancellationToken).ConfigureAwait(false);

                throw new PairActionFailureException(
                    PairActionType.AddressAttorneyAgents,
                    errorMsg.ReasonCode,
                    errorMsg.Result?.Message ?? errorMsg.Message);
            }

            return await response.ResultAsAsync<AttorneyAgentsContract>(cancellationToken);
        }

        public async Task<IRequestResult<DocumentFilesInfoDataContract?>> GetDocumentsInfo(PairNumber pairNumber, CancellationToken cancellationToken)
        {
            var url = $"{_settings.DocumentsMetadataUrl}{pairNumber.Number}";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            requestMessage.Headers.Add("Referer", $"https://patentcenter.uspto.gov/applications/{pairNumber.Number}/ifw/docs");
            requestMessage.Headers.Add("ReferrerId", UsptoHelper.InitReferrerId());

            var response = await SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            return await response.ResultAsAsync<DocumentFilesInfoDataContract>(cancellationToken).ConfigureAwait(false);
        }

        public async Task<IRequestResult<PostDocumentsRequestResponseDataContract?>> PostFilesRequestAsync(PairNumber pairNumber, PostDocumentsRequestRequest request, CancellationToken cancellationToken)
        {
            var url = _settings.DocumentsUrl;
            var jsonData = JsonConvert.SerializeObject(request);
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

            InitializeHeaders(requestMessage, pairNumber.Number);

            requestMessage.Headers.Add("Accept", "application/json, text/plain, */*");

            requestMessage.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            return await response.ResultAsAsync<PostDocumentsRequestResponseDataContract>(cancellationToken).ConfigureAwait(false);
        }

        public async Task<IRequestResult<Stream?>> DownloadFiles(PairNumber pairNumber, string requestIdentifier, CancellationToken cancellationToken)
        {
            var tokenValue = GetToken();

            var url = $"{_settings.DocumentsUrl}?requestIdentifier={requestIdentifier}&directStream=true&t={tokenValue}";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            InitializeHeaders(requestMessage, pairNumber.Number, requestIdentifier);

            requestMessage.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");

            var response = await SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            return await response.ResultAsAsync<Stream>(cancellationToken).ConfigureAwait(false);
        }

        private string GetToken()
        {
            return _token;
        }

        private void ValidateToken(HttpResponseHeaders headers)
        {
            var authToken = headers.GetValue("X-AUTH-TOKEN");

            if (string.IsNullOrEmpty(_token)
                || (!string.IsNullOrEmpty(authToken)
            && !string.IsNullOrEmpty(_token)
                && !UsptoHelper.DecodeToken(_token).IsValid()))
            {
                _token = authToken;
            }
        }

        Dictionary<UsptoSearchNumberType, string> UrlQueryParameter = new Dictionary<UsptoSearchNumberType, string>()
        {
            { UsptoSearchNumberType.ApplicationNumber, "applicationNumberText" },
            { UsptoSearchNumberType.PatentNumber, "patentNumber" },
            { UsptoSearchNumberType.PctNumber, "pctApplicationNumberText" },
            { UsptoSearchNumberType.PublicationNumber, "publicationNumber" },
            { UsptoSearchNumberType.IntDesignReg, "internationalRegistrationNumber" }
        };

        #endregion
    }
}