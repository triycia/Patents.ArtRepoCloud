using Patents.ArtRepoCloud.Domain.IntegrationEvents;
using MediatR;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Patents.ArtRepoCloud.Domain.Settings;
using Patents.ArtRepoCloud.Domain.Extensions;
using Vikcher.Framework.Data.Cosmos;
using System.IO.Compression;

namespace Patents.ArtRepoCloud.Service.Application.IntegrationEventHandlers
{
    internal class FilesZipIntegrationEventHandler : INotificationHandler<FilesZipIntegrationEvent>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IDocumentRepository _patentRepository;
        private readonly IDirectorySettings _settings;
        private readonly ILogger<FilesZipIntegrationEventHandler> _logger;

        public FilesZipIntegrationEventHandler(
            IFileRepository fileRepository, 
            IDocumentRepository patentRepository,
            IDirectorySettings settings, 
            ILogger<FilesZipIntegrationEventHandler> logger)
        {
            _fileRepository = fileRepository;
            _patentRepository = patentRepository;
            _settings = settings;
            _logger = logger;
        }

        public async Task Handle(FilesZipIntegrationEvent evt, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Handling {nameof(FilesZipIntegrationEvent)} fro references {string.Join(", ", evt.ReferenceNumbers)}.");

            var zipBlobPath = _fileRepository.ZipPath();
            var zipHash = evt.ReferenceNumbers.ToArray().CalculateHash();

            var isExist = await _fileRepository.IsExistAsync($"{zipBlobPath}{zipHash}", false, cancellationToken).ConfigureAwait(false);

            if (!isExist)
            {
                var documents = await _patentRepository.QueryDocuments()
                    .Where(d => evt.ReferenceNumbers.Contains(d.ReferenceNumber))
                    .ToListAsync(cancellationToken);

                documents = documents.Where(d => d.DocumentFile != null).ToList();

                if (documents.Any())
                {
                    using var memoryStream = new MemoryStream();
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var document in documents)
                        {
                            var path = _fileRepository.RootPath($"{document.DocumentFile!.BlobPath}{document.DocumentFile.Guid}");

                            await using var fileStream = await _fileRepository.GetAsync(path, cancellationToken).ConfigureAwait(false);
                            if (fileStream != null)
                            {
                                _logger.LogDebug($"Adding file {document.DocumentFile.Guid} {document.DocumentFile.FileName} to zip");

                                using var ms = new MemoryStream();
                                await fileStream.CopyToAsync(ms, cancellationToken);
                                var entry = archive.CreateEntry(document.DocumentFile.FileName, CompressionLevel.Fastest);
                                await using var entryStream = entry.Open();
                                var bytes = ms.ToArray();
                                await entryStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
                            }
                        }
                    }

                    memoryStream.Seek(0, SeekOrigin.Begin);

                    await _fileRepository.SaveAsync(memoryStream, zipBlobPath, zipHash, cancellationToken);
                }
            }
        }
    }
}