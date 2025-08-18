using Patents.ArtRepoCloud.Domain.IntegrationEvents;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Patents.ArtRepoCloud.Domain.Settings.EventBusSettings;
using Vikcher.Framework.EventBus.Abstractions.Queue;
using MediatR;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.DocumentFileImageExtraction
{
    public class DocumentFileImageExtractionCommandHandler : IRequestHandler<DocumentFileImageExtractionCommand, DocumentFileImageExtractionCommandResult>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IDocumentRepository _patentRepository;
        private readonly IEventBusQueueClient<ArtRepoFileServiceBusQueueClientSettings> _busArchiveQueueClient;
        private readonly ILogger<DocumentFileImageExtractionCommandHandler> _logger;

        public DocumentFileImageExtractionCommandHandler(IFileRepository fileRepository, IDocumentRepository patentRepository, IEventBusQueueClient<ArtRepoFileServiceBusQueueClientSettings> busArchiveQueueClient, ILogger<DocumentFileImageExtractionCommandHandler> logger)
        {
            _fileRepository = fileRepository;
            _patentRepository = patentRepository;
            _busArchiveQueueClient = busArchiveQueueClient;
            _logger = logger;
        }

        public async Task<DocumentFileImageExtractionCommandResult> Handle(DocumentFileImageExtractionCommand command, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting {nameof(DocumentFileImageExtractionCommand)} for reference # {command.ReferenceNumber}");

            var document = await _patentRepository
                .GetByReferenceNumberAsync(command.ReferenceNumber, cancellationToken)
                .ConfigureAwait(false);

            if (document == null)
            {
                var msg = $"{nameof(DocumentFileImageExtractionCommand)} failed! No document found for ReferenceNumber: {command.ReferenceNumber}";

                _logger.LogWarning(msg);

                throw new BadHttpRequestException(msg);
            }

            if (document.DocumentFile == null)
            {
                var msg = $"{nameof(DocumentFileImageExtractionCommand)} failed! No Pdf file found for ReferenceNumber: {command.ReferenceNumber}";

                _logger.LogWarning(msg);

                return new DocumentFileImageExtractionCommandResult(false);
            }

            var filePath = _fileRepository.RootPath($"{document.DocumentFile.BlobPath}{document.DocumentFile.Guid}");

            var fileStream = await _fileRepository.GetAsync(filePath, cancellationToken).ConfigureAwait(false);

            if (fileStream == null)
            {
                var msg = $"{nameof(DocumentFileImageExtractionCommand)} failed! Pdf file not found for ReferenceNumber: {command.ReferenceNumber}";

                _logger.LogWarning(msg);

                return new DocumentFileImageExtractionCommandResult(false);
            }

            var relativeTemporaryBlobPath = $"{DateTime.Now:yyyyMMdd}/{command.ReferenceNumber.ToLower()}/";
            var destinationPath = _fileRepository.TempPath(relativeTemporaryBlobPath);

            await _fileRepository.SaveAsync(fileStream, destinationPath, document.DocumentFile.Guid.ToString(), cancellationToken);
            
            await _busArchiveQueueClient.Enqueue(new ImageExtractionIntegrationEvent(
                $"{destinationPath}{document.DocumentFile.Guid}",
                destinationPath,
                command.Pages
                )).ConfigureAwait(false);

            return new DocumentFileImageExtractionCommandResult(
                true, 
                document.DocumentFile.Guid, 
                relativeTemporaryBlobPath, 
                document.DocumentFile.PageCount);
        }
    }
}