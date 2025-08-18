using Patents.ArtRepoCloud.Domain.Interfaces;
using MediatR;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.DeleteDocumentFile
{
    public class DeleteDocumentFileCommandHandler : IRequestHandler<DeleteDocumentFileCommand, DeleteDocumentFileCommandResult>
    {
        private readonly IDocumentRepository _patentRepository;
        private readonly IFileRepository _fileRepository;
        private readonly ILogger<DeleteDocumentFileCommandHandler> _logger;

        public DeleteDocumentFileCommandHandler(IDocumentRepository patentRepository, IFileRepository fileRepository, ILogger<DeleteDocumentFileCommandHandler> logger)
        {
            _patentRepository = patentRepository;
            _fileRepository = fileRepository;
            _logger = logger;
        }

        public async Task<DeleteDocumentFileCommandResult> Handle(DeleteDocumentFileCommand command, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting {nameof(DeleteDocumentFileCommand)} for Reference # {command.ReferenceNumber}");

            try
            {
                var document = await _patentRepository.GetByReferenceNumberAsync(command.ReferenceNumber, cancellationToken).ConfigureAwait(false);

                if (document == null || document.DocumentFile == null)
                {
                    throw new BadHttpRequestException($"Failed to delete file for Reference # {command.ReferenceNumber}. Not found!");
                }

                var path = _fileRepository.RootPath($"{document.DocumentFile.BlobPath}{document.DocumentFile.Guid}");

                var isExist = await _fileRepository.IsExistAsync(path, false, cancellationToken).ConfigureAwait(false);

                if (!isExist)
                {
                    throw new BadHttpRequestException($"File not found for Reference # {command.ReferenceNumber}");
                }

                await _fileRepository.DeleteAsync(path, cancellationToken).ConfigureAwait(false);

                document.RemoveDocumentFile();

                _patentRepository.Update(document, cancellationToken);

                await _patentRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                return new DeleteDocumentFileCommandResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed {nameof(DeleteDocumentFileCommand)} for Reference # {command.ReferenceNumber}");
            }

            return new DeleteDocumentFileCommandResult(false);
        }
    }
}