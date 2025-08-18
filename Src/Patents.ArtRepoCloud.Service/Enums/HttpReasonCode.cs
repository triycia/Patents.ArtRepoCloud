namespace Patents.ArtRepoCloud.Service.Enums
{
    public enum HttpReasonCode
    {
        None = 0,
        Success = 1,
        Failed = 2,
        ToBeContinued = 3,
        NoData = 4,
        BadData = 5,
        BadRequest = 6,
        ParsingError = 7,
        TooManyRequests = 8,
        RequestTimeout = 9,
        NotFound = 10,
        InternalError = 11,
        FetchSourceUnavailable = 12
    }
}