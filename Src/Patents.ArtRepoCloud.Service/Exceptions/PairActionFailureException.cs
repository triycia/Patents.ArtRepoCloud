using Patents.ArtRepoCloud.Service.Enums;

namespace Patents.ArtRepoCloud.Service.Exceptions
{
    internal class PairActionFailureException : Exception
    {
        public PairActionFailureException() : base() { }
        public PairActionFailureException(string message) : base(message) { }

        public PairActionFailureException(
            PairActionType taskType,
            HttpReasonCode reasonCode,
            string? message)
        {
            TaskType = taskType;
            ReasonCode = reasonCode;
            Message = message;
        }

        public PairActionType TaskType { get; }
        public HttpReasonCode ReasonCode { get; }
        public string? Message { get; }
    }
}