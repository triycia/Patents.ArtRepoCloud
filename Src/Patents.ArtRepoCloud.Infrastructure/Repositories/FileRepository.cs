using Patents.ArtRepoCloud.Domain.Interfaces;
using Patents.ArtRepoCloud.Domain.Settings;
using Microsoft.Extensions.Logging;
using Vikcher.Framework.IO.FileProxy;

namespace Patents.ArtRepoCloud.Infrastructure.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly IFileApiProxy _fileApiProxy;
        private readonly IDirectorySettings _settings;
        private readonly ILogger<FileRepository> _logger;

        public FileRepository(IFileApiProxy fileApiProxy, IDirectorySettings settings, ILogger<FileRepository> logger)
        {
            _fileApiProxy = fileApiProxy;
            _settings = settings;
            _logger = logger;
        }

        public async Task<Stream?> GetAsync(string path, CancellationToken cancellationToken)
        {
            try
            {
                return await _fileApiProxy.GetAsync(path, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError($"No File found at path: {path}");

                return null;
            }
        }

        public async Task<Guid> SaveAsync(Stream stream, string path, CancellationToken cancellationToken)
        {
            var onDiskId = Guid.NewGuid();

            path = $"{path}{onDiskId}";

            await _fileApiProxy.CreateAsync(path, stream, cancellationToken).ConfigureAwait(false);

            return onDiskId;
        }

        public async Task SaveAsync(Stream stream, string path, string onDiskId, CancellationToken cancellationToken)
        {
            path = $"{path}{onDiskId}";

            await _fileApiProxy.CreateAsync(path, stream, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(string path, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Deleting file at path: {path}.");

            await _fileApiProxy.DeleteAsync(path, cancellationToken).ConfigureAwait(false);
        }

        public async Task FinalizeTempFileAsync(string relativePath, CancellationToken cancellationToken)
        {
            var tempPath = TempPath(relativePath);
            var rootPath = RootPath(relativePath);

            await using var stream = await _fileApiProxy.GetAsync(tempPath, cancellationToken).ConfigureAwait(false);

            _logger.LogDebug($"Copying a file {tempPath} to {rootPath}.");

            await _fileApiProxy.CreateAsync(rootPath, stream, cancellationToken).ConfigureAwait(false);

            await _fileApiProxy.DeleteAsync(tempPath, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> IsExistAsync(string path, bool includeSize, CancellationToken cancellationToken)
        {
            return await _fileApiProxy.IsExistAsync(path, includeSize, cancellationToken).ConfigureAwait(false);
        }

        public string TempPath(string relativePath) => $"{_settings.TempDirectory}{relativePath}";
        public string RootPath(string relativePath) => $"{_settings.ArtDirectory}{relativePath}";
        public string ZipPath(string? relativePath = null) => $"{_settings.ZipDirectory}{relativePath}";
        public string BibDataPath(string relativePath) => $"{_settings.JsonDirectory}{relativePath}";
    }
}