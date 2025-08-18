using Patents.ArtRepoCloud.Service.Enums;
using Newtonsoft.Json;
using System.Net;
using Patents.ArtRepoCloud.Service.Factories.HttpFactory.Interfaces;
using Patents.ArtRepoCloud.Service.Extensions;

namespace Patents.ArtRepoCloud.Service.Factories.HttpFactory
{
    public static class HttpExtensions
    {
        public static async Task<IRequestResult<TResult?>> ResponseAsAsync<TResult>(this HttpResponseMessage response, CancellationToken cancellationToken)
        {
            HttpReasonCode reason = response.StatusCode.ToReasonCode();
            TResult? tResult = default;

            try
            {
                var jsonStr = await response.Content.ReadAsStringAsync(cancellationToken);

                if (string.IsNullOrEmpty(jsonStr))
                {
                    reason = HttpReasonCode.NoData;
                }
                else
                {
                    tResult = JsonConvert.DeserializeObject<TResult>(jsonStr);

                    if (tResult == null)
                        reason = HttpReasonCode.ParsingError;
                }
            }
            catch
            {
                reason = HttpReasonCode.ParsingError;
            }

            return new RequestResult<TResult?>(tResult, reason, reason == HttpReasonCode.Success, reason.GetName()!);
        }

        public static async Task<IRequestResult<TResult?>> ResultAsAsync<TResult>(this HttpResponseMessage response, CancellationToken cancellationToken)
        {
            HttpReasonCode reason = response.StatusCode.ToReasonCode();

            TResult? tResult = default;

            if (reason != HttpReasonCode.Success)
            {
                return new RequestResult<TResult>(tResult, reason, false, reason.GetName()!);
            }

            try
            {
                if (typeof(TResult).IsAssignableFrom(typeof(Stream)) && response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

                    IRequestResult<Stream> result = new RequestResult<Stream>(stream, reason, true, reason.GetName()!);

                    return (IRequestResult<TResult>)result;
                }

                var jsonStr = await response.Content.ReadAsStringAsync(cancellationToken);

                if (string.IsNullOrEmpty(jsonStr))
                {
                    reason = HttpReasonCode.NoData;
                }
                else
                {
                    tResult = JsonConvert.DeserializeObject<TResult>(jsonStr);

                    if (tResult == null)
                        reason = HttpReasonCode.ParsingError;
                }
            }
            catch
            {
                reason = HttpReasonCode.ParsingError;
            }

            return new RequestResult<TResult?>(tResult, reason, reason == HttpReasonCode.Success, reason.GetName()!);
        }

        public static HttpReasonCode ToReasonCode(this HttpStatusCode statusCode)
        {
            int num = (int)statusCode / 100;

            switch (num)
            {
                case 2:
                    return HttpReasonCode.Success;
                case 4:
                    if (statusCode == HttpStatusCode.TooManyRequests)
                    {
                        return HttpReasonCode.TooManyRequests;
                    }

                    if (statusCode == HttpStatusCode.NotFound)
                    {
                        return HttpReasonCode.NotFound;
                    }

                    return HttpReasonCode.BadRequest;
                case 5:
                    return HttpReasonCode.InternalError;
                default:
                    return HttpReasonCode.Failed;
            }
        }
    }
}