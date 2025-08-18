namespace Patents.ArtRepoCloud.Service.DataProviders.Uspto.Contracts
{
    public class ErrorMessage
    {
        public ErrorMessage(
            string? message,
            string? level,
            int status,
            string? error,
            DateTime? timestamp,
            string? field)
        {
            Message = message;
            Level = level;
            Status = status;
            Error = error;
            Timestamp = timestamp;
            Field = field;
        }

        public string? Message { get; }
        public string? Level { get; }
        public int Status { get; }
        public string? Error { get; }
        public DateTime? Timestamp { get; }
        public string? Field { get; }
    }
}