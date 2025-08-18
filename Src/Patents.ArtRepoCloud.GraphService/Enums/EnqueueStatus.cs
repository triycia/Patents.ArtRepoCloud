namespace Patents.ArtRepoCloud.GraphService.Enums
{
    public enum EnqueueStatus
    {
        Success = 1,
        Duplicate = 2,
        AlreadyExist = 3,
        ParseError = 4,
        InternalServerError = 5
    }
}