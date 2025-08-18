using Patents.ArtRepoCloud.Domain.Interfaces;
using Patents.ArtRepoCloud.Domain.Settings.EventBusSettings;
using Vikcher.Framework.EventBus.Abstractions.Queue;
using MediatR;
using Patents.ArtRepoCloud.Domain.IntegrationEvents;
using Vikcher.Framework.Common;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.ImageExtraction
{
    public class ImageExtractionCommandHandler : IRequestHandler<ImageExtractionCommand, ImageExtractionCommandResult>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IEventBusQueueClient<ArtRepoFileServiceBusQueueClientSettings> _busArchiveQueueClient;
        private readonly ILogger<ImageExtractionCommandHandler> _logger;

        public ImageExtractionCommandHandler(
            IFileRepository fileRepository, 
            IEventBusQueueClient<ArtRepoFileServiceBusQueueClientSettings> busArchiveQueueClient, 
            ILogger<ImageExtractionCommandHandler> logger)
        {
            _fileRepository = fileRepository;
            _busArchiveQueueClient = busArchiveQueueClient;
            _logger = logger;
        }

        public async Task<ImageExtractionCommandResult> Handle(ImageExtractionCommand command, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting {nameof(ImageExtractionCommand)} for: {command.ToJson()}.");

            var pdfFilePath = $"{command.RelativeBlobPath}{command.OnDiskGuid}";

            var isExist = await _fileRepository.IsExistAsync(pdfFilePath, false, cancellationToken).ConfigureAwait(false);

            if (isExist)
            {
                await _busArchiveQueueClient.Enqueue(new ImageExtractionIntegrationEvent(pdfFilePath, command.RelativeBlobPath, command.Pages)).ConfigureAwait(false);

                return new ImageExtractionCommandResult(true);
            }

            return new ImageExtractionCommandResult(false);
        }
    }
}