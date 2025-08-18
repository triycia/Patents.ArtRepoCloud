using Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Patents.ArtRepoCloud.Service.Extensions;
using MediatR;

namespace Patents.ArtRepoCloud.Service.Application.Commands.SaveArtRepoDocumentPdfs
{
    class SaveArtRepoDocumentPdfsCommandHandler : IRequestHandler<SaveArtRepoDocumentPdfsCommand, SaveArtRepoDocumentPdfsCommandResult>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IDocumentRepository _patentRepository;
        private readonly ILogger<SaveArtRepoDocumentPdfsCommandHandler> _logger;

        public SaveArtRepoDocumentPdfsCommandHandler(IFileRepository fileRepository, IDocumentRepository patentRepository, ILogger<SaveArtRepoDocumentPdfsCommandHandler> logger)
        {
            _fileRepository = fileRepository;
            _patentRepository = patentRepository;
            _logger = logger;
        }

        public async Task<SaveArtRepoDocumentPdfsCommandResult> Handle(SaveArtRepoDocumentPdfsCommand command, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting {nameof(SaveArtRepoDocumentPdfsCommand)} for reference # {command.ReferenceNumber}.");

            try
            {
                var document = await _patentRepository.GetByReferenceNumberAsync(command.ReferenceNumber.SourceReferenceNumber, cancellationToken)
                    .ConfigureAwait(false);

                if (document == null)
                {
                    _logger.LogWarning($"{nameof(SaveArtRepoDocumentPdfsCommand)} failed for reference # {command.ReferenceNumber}. Document does not exists in our system.");

                    throw new InvalidOperationException($"{nameof(SaveArtRepoDocumentPdfsCommand)} failed for reference # {command.ReferenceNumber}. Document does not exists in our system.");
                }

                var fileData = command.PdfFiles.Single();

                var relativeFilePath = $"{fileData.FilePath}{fileData.OnDiskId}";

                await _fileRepository
                    .FinalizeTempFileAsync(relativeFilePath, cancellationToken)
                    .ConfigureAwait(false);

                var currentFile = document.DocumentFile;
                var blobDataPath = currentFile?.BlobPath != null ? _fileRepository.RootPath($"{currentFile.BlobPath}{currentFile.Guid}") : null;

                if (blobDataPath != null && await IsExist(blobDataPath).ConfigureAwait(false))
                {
                    await _fileRepository.DeleteAsync(blobDataPath, cancellationToken).ConfigureAwait(false);
                }

                document.SetDocumentFile(new DocumentFile(
                    fileData.OnDiskId,
                    $"{command.ReferenceNumber.SourceReferenceNumber}.pdf",
                    fileData.MediaType,
                    fileData.FilePath,
                    fileData.PageCount,
                    fileData.Length,
                    command.Source.ToDataSource(),
                    DateTime.Now));

                _patentRepository.Update(document, cancellationToken);

                await _patentRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                Task<bool> IsExist(string path) => _fileRepository.IsExistAsync(path, false, cancellationToken);

                return new SaveArtRepoDocumentPdfsCommandResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(SaveArtRepoDocumentPdfsCommand)} failed for reference # {command.ReferenceNumber}.");

                return new SaveArtRepoDocumentPdfsCommandResult(false);
            }
        }
    }
}