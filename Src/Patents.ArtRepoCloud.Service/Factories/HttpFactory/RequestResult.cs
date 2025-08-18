using Patents.ArtRepoCloud.Service.Enums;
using Patents.ArtRepoCloud.Service.Factories.HttpFactory.Interfaces;

namespace Patents.ArtRepoCloud.Service.Factories.HttpFactory
{
    public class RequestResult<T> : IRequestResult<T>
    {
        public RequestResult(HttpReasonCode reasonCode, bool success, string message)
        {
            ReasonCode = reasonCode;
            Success = success;
            Message = message;
        }
        public RequestResult(T? result, HttpReasonCode reasonCode, bool success, string message)
        {
            Result = result;
            ReasonCode = reasonCode;
            Success = success;
            Message = message;
        }

        public T? Result { get; }
        public HttpReasonCode ReasonCode { get; }
        public bool Success { get; }
        public string Message { get; }
    }
}