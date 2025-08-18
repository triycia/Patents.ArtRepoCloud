using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Service.Enums;
using Patents.ArtRepoCloud.Service.Extensions;

namespace Patents.ArtRepoCloud.Service.Exceptions
{
    public class ImportProcessFailureReasonException : Exception
    {
        public ImportProcessFailureReasonException() : base() { }
        public ImportProcessFailureReasonException(string message) : base(message) { }

        public ImportProcessFailureReasonException(
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