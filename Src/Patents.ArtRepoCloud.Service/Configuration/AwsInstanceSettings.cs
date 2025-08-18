namespace Patents.ArtRepoCloud.Service.Configuration
{
    internal class AwsInstanceSettings
    {
        public bool EnableEc2Manager { get; init; }
        public DateTime StartTime { get; init; }
        public DateTime StopTime { get; init; }
        public bool StopOnWeekend { get; init; }
        public int Timeout { get; init; }
        public int MaximumDocumentsCount { get; init; }
        public int MaximumProcessCount { get; init; }
        public int MaxUnitsOfWork { get; init; }
    }
}