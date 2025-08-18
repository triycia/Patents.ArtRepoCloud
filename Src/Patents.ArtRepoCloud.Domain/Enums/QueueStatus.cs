namespace Patents.ArtRepoCloud.Domain.Enums
{
    public enum QueueStatus
    {
        None = 0,
        Queued = 1,
        InProcess = 2,
        NoData = 3,
        Failed = 4,
        Completed = 5,
        ToBeContinued = 6
    }
}