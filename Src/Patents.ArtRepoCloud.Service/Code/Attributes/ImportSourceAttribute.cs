using Patents.ArtRepoCloud.Domain.Enums;

namespace Patents.ArtRepoCloud.Service.Code.Attributes
{
    internal class ImportSourceAttribute : Attribute
    {
        public ImportSourceAttribute(ImportSource source)
        {
            Source = source;
        }

        public ImportSource Source { get; }
    }
}