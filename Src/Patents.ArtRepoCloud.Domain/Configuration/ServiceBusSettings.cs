namespace Patents.ArtRepoCloud.Domain.Configuration
{
    public class ServiceBusSettings
    {
        public string ConnectionString { get; init; }
        public Priority ArtRepoQueue { get; init; }
        public Priority PAIRQueue { get; init; }
        public string ArtRepoFileQueue { get; init; }

        public class Priority
        {
            public string HighPriority { get; init; }
            public string NormalPriority { get; init; }
            public string LowPriority { get; init; }
            public string IdlePriority { get; init; }
        }
    }
}