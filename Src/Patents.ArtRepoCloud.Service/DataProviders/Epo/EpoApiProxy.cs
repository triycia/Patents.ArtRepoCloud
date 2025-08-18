using Vikcher.Framework.Common;
using Patents.ArtRepoCloud.Service.Configuration;
using Patents.ArtRepoCloud.Service.DataProviders.Epo.Interfaces;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.DataFetchers;
using System.Xml.Linq;

namespace Patents.ArtRepoCloud.Service.DataProviders.Epo
{
    public class EpoApiProxy : IEpoApiProxy
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly EpoSettings _settings;
        private static string _token = string.Empty;
        private readonly string _accessTokenPattern = $@"""access_token""\s*:\s*""(?<accessToken>[^""]*)""";

        public EpoApiProxy(IHttpClientFactory clientFactory, EpoSettings settings)
        {
            _clientFactory = clientFactory;
            _settings = settings;
        }

        private async Task<HttpClient> CreateClient(string acceptHeader, CancellationToken cancellationToken)
        {
            var client = _clientFactory.CreateClient("EpoService");

            if (string.IsNullOrEmpty(_token))
            {
                await Authenticate(client, cancellationToken).ConfigureAwait(false);
            }

            client.DefaultRequestHeaders.Add("Authorization", _token);

            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue(acceptHeader));

            return client;
        }

        public async Task<ImageXMetadata> GetImageMetadataAsync(ReferenceNumber referenceNumber, CancellationToken cancellationToken)
        {
            var searchNumber = FormatReferenceNumber(referenceNumber);
            var url = BuildPublishedDataServiceUri("images", searchNumber);

            var client = await CreateClient("application/json", cancellationToken).ConfigureAwait(false);
            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), cancellationToken).ConfigureAwait(false);
            var xmlData = await response.Content.ReadAsStringAsync(cancellationToken);

            return new ImageXMetadata(xmlData);
        }

        public async Task<Stream> GetAttachmentAsync(string link, int pageNumber, string mediaType, CancellationToken cancellationToken)
        {
            var url = _settings.PageLinkQueryString.Replace("{command}", link);

            var client = await CreateClient("application/pdf", cancellationToken).ConfigureAwait(false);
            var requestMsg = new HttpRequestMessage(HttpMethod.Get, url);

            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue(mediaType));//ACCEPT header

            requestMsg.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

            requestMsg.Headers.Add("X-OPS-Range", pageNumber.ToString());

            var response = await client.SendAsync(requestMsg, cancellationToken).ConfigureAwait(false);

            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }

        public async Task<EpoXDocument> GetBibDocumentAsync(ReferenceNumber referenceNumber, CancellationToken cancellationToken)
        {
            var searchNumber = FormatReferenceNumber(referenceNumber);
            var url = BuildPublishedDataServiceUri("biblio", searchNumber);

            var client = await CreateClient("application/json", cancellationToken).ConfigureAwait(false);
            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), cancellationToken).ConfigureAwait(false);
            var xmlData = await response.Content.ReadAsStringAsync(cancellationToken);

            return await EpoXDocument.LoadAsync(xmlData, LoadOptions.None, referenceNumber.NumberType, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetDescriptionAsync(ReferenceNumber referenceNumber, CancellationToken cancellationToken)
        {
            var searchNumber = FormatReferenceNumber(referenceNumber);

            var url = BuildPublishedDataServiceUri("description", searchNumber);

            var client = await CreateClient("application/json", cancellationToken).ConfigureAwait(false);
            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), cancellationToken).ConfigureAwait(false);
            var xmlData = await response.Content.ReadAsStringAsync(cancellationToken);

            return xmlData;
        }

        public async Task<string> GetClaimsAsync(ReferenceNumber referenceNumber, CancellationToken cancellationToken)
        {
            var searchNumber = FormatReferenceNumber(referenceNumber);

            var url = BuildPublishedDataServiceUri("claims", searchNumber);

            var client = await CreateClient("application/json", cancellationToken).ConfigureAwait(false);
            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), cancellationToken).ConfigureAwait(false);
            var xmlData = await response.Content.ReadAsStringAsync(cancellationToken);

            return xmlData;
        }

        async Task Authenticate(HttpClient client, CancellationToken cancellationToken)
        {
            try
            {
                var creds = $"{Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_settings.EpoKey}:{_settings.EpoSecret}"))}";

                var authRequestMsg = new HttpRequestMessage(HttpMethod.Post, $"{_settings.EpoServiceUrl}auth/accesstoken");
                authRequestMsg.Headers.Add("Authorization", $"Basic {creds}");
                authRequestMsg.Content = new StringContent("grant_type=client_credentials", Encoding.ASCII, "application/x-www-form-urlencoded");

                var response = await client.SendAsync(authRequestMsg, cancellationToken).ConfigureAwait(false);

                if (!response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    var str = await response.Content.ReadAsStringAsync(cancellationToken);
                }



                var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                var accessTokenRegEx = new Regex(_accessTokenPattern);

                var match = accessTokenRegEx.Match(Encoding.UTF7.GetString(bytes));

                if (match.Success)
                {
                    _token = $"Bearer {match.Groups["accessToken"].Value}";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        string BuildPublishedDataServiceUri(string command, string referenceNumber)
        {
            Guard.AssertNotNullOrEmptyOrWhiteSpace(command, nameof(command));
            Guard.AssertNotNullOrEmptyOrWhiteSpace(referenceNumber, nameof(referenceNumber));
            var siteUrl = _settings.PublishedDataQueryString;
            siteUrl = siteUrl.Replace("{referenceNumber}", referenceNumber);
            siteUrl = siteUrl.Replace("{command}", command);
            return siteUrl;
        }

        static string FormatReferenceNumber(ReferenceNumber referenceNumber)
        {
            return referenceNumber.CountryCode.Equals("EP", StringComparison.CurrentCultureIgnoreCase) || referenceNumber.CountryCode.Equals("WO") ? (referenceNumber.KindCode.Equals(string.Empty) ? referenceNumber.SourceReferenceNumber : referenceNumber.SeparatorFormat) : referenceNumber.ToString();
        }
    }
}