using Patents.ArtRepoCloud.Domain.Interfaces;
using MediatR;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.DeleteImage
{
    public class DeleteImageCommandHandler : IRequestHandler<DeletedDocumentImageCommand, DeleteImageCommandResult>
    {
        private readonly IDocumentRepository _patentRepository;
        private readonly IFileRepository _fileRepository;
        private readonly ILogger<DeleteImageCommandHandler> _logger;

        public DeleteImageCommandHandler(IDocumentRepository patentRepository, IFileRepository fileRepository, ILogger<DeleteImageCommandHandler> logger)
        {
            _patentRepository = patentRepository;
            _fileRepository = fileRepository;
            _logger = logger;
        }

        public async Task<DeleteImageCommandResult> Handle(DeletedDocumentImageCommand command, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting {nameof(DeletedDocumentImageCommand)} for Reference # {command.ReferenceNumber}");

            try
            {
                var document = await _patentRepository.GetByReferenceNumberAsync(command.ReferenceNumber, cancellationToken).ConfigureAwait(false);

                if (document == null)
                {
                    _logger.LogWarning($"Failed {nameof(DeletedDocumentImageCommand)} for Reference # {command.ReferenceNumber}. Document not found.");

                    return new DeleteImageCommandResult(false);
                }

                var imageData = document.DocumentImages.FirstOrDefault(x => x.Guid == command.ImageGuid);

                if (imageData != null)
                {
                    var path = _fileRepository.RootPath($"{imageData.BlobPath}/{imageData.Guid}");

                    var isExist = await _fileRepository.IsExistAsync(path, false, cancellationToken).ConfigureAwait(false);

                    if (!isExist)
                    {
                        throw new BadHttpRequestException($"Document image Guid/FileName {imageData.Guid}/{imageData.FileName} not found for Reference # {command.ReferenceNumber}");
                    }

                    await _fileRepository.DeleteAsync(path, cancellationToken).ConfigureAwait(false);

                    document.RemoveDocumentImage(imageData);

                    _patentRepository.Update(document, cancellationToken);

                    await _patentRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    return new DeleteImageCommandResult(true);
                }

                _logger.LogWarning($"Failed {nameof(DeletedDocumentImageCommand)} for Reference # {command.ReferenceNumber}. Document image not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed {nameof(DeletedDocumentImageCommand)} for Reference # {command.ReferenceNumber}");
            }

            return new DeleteImageCommandResult(false);
        }
    }
}