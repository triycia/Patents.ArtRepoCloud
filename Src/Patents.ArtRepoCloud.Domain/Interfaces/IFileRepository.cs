using Patents.ArtRepoCloud.Domain.ValueObjects;

namespace Patents.ArtRepoCloud.Domain.Interfaces
{
    public interface IFileRepository
    {
        Task<Stream?> GetAsync(string path, CancellationToken cancellationToken);
        Task<Guid> SaveAsync(Stream stream, string path, CancellationToken cancellationToken);
        Task SaveAsync(Stream stream, string path, string onDiskId, CancellationToken cancellationToken);
        Task DeleteAsync(string path, CancellationToken cancellationToken);
        Task FinalizeTempFileAsync(string relativePath, CancellationToken cancellationToken);
        Task<bool> IsExistAsync(string path, bool includeSize, CancellationToken cancellationToken);

        string TempPath(string relativePath);
        string RootPath(string relativePath);
        string ZipPath(string? relativePath = null);
        string BibDataPath(string relativePath);
    }
}