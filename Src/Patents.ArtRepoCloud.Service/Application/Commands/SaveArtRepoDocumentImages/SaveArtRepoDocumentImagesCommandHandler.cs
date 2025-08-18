using Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate;
using Patents.ArtRepoCloud.Domain.IntegrationEvents;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Patents.ArtRepoCloud.Service.Extensions;
using MediatR;

namespace Patents.ArtRepoCloud.Service.Application.Commands.SaveArtRepoDocumentImages
{
    class SaveArtRepoDocumentImagesCommandHandler : IRequestHandler<SaveArtRepoDocumentImagesCommand, SaveArtRepoDocumentImagesCommandResult>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IDocumentRepository _patentRepository;
        private readonly ILogger<SaveArtRepoDocumentImagesCommandHandler> _logger;

        public SaveArtRepoDocumentImagesCommandHandler(IFileRepository fileRepository, IDocumentRepository patentRepository, ILogger<SaveArtRepoDocumentImagesCommandHandler> logger)
        {
            _fileRepository = fileRepository;
            _patentRepository = patentRepository;
            _logger = logger;
        }

        public async Task<SaveArtRepoDocumentImagesCommandResult> Handle(SaveArtRepoDocumentImagesCommand command, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting {nameof(SaveArtRepoDocumentImagesCommand)} for Reference # {command.ReferenceNumber}");

            try
            {
                var document = await _patentRepository.GetByReferenceNumberAsync(command.ReferenceNumber.SourceReferenceNumber, cancellationToken)
                    .ConfigureAwait(false);

                if (document == null)
                {
                    _logger.LogWarning($"{nameof(SaveArtRepoDocumentImagesCommand)} have failed for reference {command.ReferenceNumber}. Document not found in our system!");

                    throw new InvalidOperationException(
                        $"{nameof(ArtRepoImageIntegrationEvent)} have failed for reference {command.ReferenceNumber}. Document not found in our system!");
                }

                var tasks = command.Files.Select(x => _fileRepository.FinalizeTempFileAsync($"{x.FilePath}{x.OnDiskId}", cancellationToken));

                await Task.WhenAll(tasks).ConfigureAwait(false);

                var dateTimeNow = DateTime.Now;

                if (document.DocumentImages.Any())
                {
                    foreach (var item in document.DocumentImages)
                    {
                        var blobDataPath = _fileRepository.RootPath($"{item.BlobPath}{item.Guid}");

                        if (await IsExist(blobDataPath).ConfigureAwait(false))
                        {
                            await _fileRepository.DeleteAsync(blobDataPath, cancellationToken).ConfigureAwait(false);
                        }
                    }

                    document.DocumentImages.Clear();
                }

                foreach (var file in command.Files)
                {
                    document.AddDocumentImage(new DocumentImage(
                        file.OnDiskId,
                        file.FileName,
                        file.MediaType,
                        file.FilePath,
                        file.Length,
                        command.Source.ToDataSource(),
                        dateTimeNow,
                        file.Sequence));
                }

                _patentRepository.Update(document, cancellationToken);

                await _patentRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogDebug($"Completed {nameof(SaveArtRepoDocumentImagesCommand)} for Reference # {command.ReferenceNumber}");

                Task<bool> IsExist(string path) => _fileRepository.IsExistAsync(path, false, cancellationToken);

                return new SaveArtRepoDocumentImagesCommandResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed {nameof(SaveArtRepoDocumentImagesCommand)} for Reference # {command.ReferenceNumber}");

                return new SaveArtRepoDocumentImagesCommandResult(false);
            }
        }
    }
}