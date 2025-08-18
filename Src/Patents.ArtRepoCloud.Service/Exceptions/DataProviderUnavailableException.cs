using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Service.Enums;
using Patents.ArtRepoCloud.Service.Extensions;

namespace Patents.ArtRepoCloud.Service.Exceptions
{
    public class DataProviderUnavailableException : Exception
    {
        public DataProviderUnavailableException(string message) : base(message) { }

        public DataProviderUnavailableException(
            ImportSource importSource,
            HttpReasonCode reasonCode,
            string? message = null)
        {
            ImportSource = importSource;
            ReasonCode = reasonCode;
            Message = message ?? ReasonCode.GetName();
        }

        public ImportSource ImportSource { get; }
        public HttpReasonCode ReasonCode { get; }
        public string? Message { get; }
    }
}