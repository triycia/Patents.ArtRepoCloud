using Patents.ArtRepoCloud.Domain.Enums;

namespace Patents.ArtRepoCloud.Domain.Attributes
{
    public class QueueClientTypeAttribute : Attribute
    {
        public ImportPriority Priority { get; }
        public ImportSource Source { get; }

        public QueueClientTypeAttribute(ImportPriority priority, ImportSource source)
        {
            Priority = priority;
            Source = source;
        }

        public bool TypeOf(ImportPriority priority, ImportSource source)
        {
            return Priority.Equals(priority) && Source.Equals(source);
        }
    }
}