using Vikcher.Framework.Common;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.Configuration;
using Patents.ArtRepoCloud.Service.DataProviders.Questel.Interfaces;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace Patents.ArtRepoCloud.Service.DataProviders.Questel
{
    public class QuestelApiProxy : IQuestelApiProxy
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly QuestelSettings _settings;

        public QuestelApiProxy(IHttpClientFactory clientFactory, QuestelSettings settings)
        {
            _clientFactory = clientFactory;
            _settings = settings;
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            var client = _clientFactory.CreateClient("QuestelService");

            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            return response;
        }

        public async Task<Stream> DownloadPdfAsync(ReferenceNumber referenceNumber, CancellationToken cancellationToken)
        {
            Guard.AssertNotNull(referenceNumber, nameof(referenceNumber));

            var response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, BuildRequestUrl(referenceNumber)), cancellationToken).ConfigureAwait(false);

            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }

        private string BuildRequestUrl(ReferenceNumber referenceNumber)
        {
            Guard.AssertNotNull(referenceNumber, nameof(referenceNumber));

            using var md5 = MD5.Create();
            var dataToHashBytes = Encoding.ASCII.GetBytes(_settings.QuestelPassword);
            var salt = BitConverter.ToString(md5.ComputeHash(dataToHashBytes)).Replace("-", string.Empty).ToLower();
            var pn = referenceNumber.CountryCode.ToUpper() + referenceNumber.Number;
            var kindCode = (referenceNumber.KindCode ?? string.Empty).ToUpper();

            if (referenceNumber.NumberType == ReferenceNumberSourceType.Wipo)
            {
                pn = $"{referenceNumber}";
                kindCode = referenceNumber.KindCode;
            }
            else
            {
                var ucidParts = referenceNumber.Ucid.ToUpper().Split('-');

                pn = ucidParts[0] + ucidParts[1];

                if (ucidParts.Length > 2)
                {
                    kindCode = ucidParts[2];
                }
            }

            var hashStr = $"{pn}|{kindCode}|{_settings.QuestelLogon.ToUpper()}|{salt}";

            dataToHashBytes = Encoding.ASCII.GetBytes(hashStr);

            var afterHash = BitConverter.ToString(md5.ComputeHash(dataToHashBytes)).Replace("-", string.Empty).ToLower();

            return $"{_settings.QuestelUrl}?pn={pn}&kind={kindCode}&logon={_settings.QuestelLogon}&hash={afterHash}";
        }
    }
}