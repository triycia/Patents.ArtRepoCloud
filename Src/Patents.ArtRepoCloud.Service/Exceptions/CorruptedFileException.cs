using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Service.Enums;

namespace Patents.ArtRepoCloud.Service.Exceptions
{
    public class CorruptedFileException : Exception
    {
        public CorruptedFileException(string message) : base(message) { }
    }
}