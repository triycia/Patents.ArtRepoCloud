using Patents.ArtRepoCloud.Domain.Interfaces;
using Patents.ArtRepoCloud.Domain.Settings;
using Vikcher.Framework.Common;
using MediatR;
using Vikcher.Framework.EventBus.Abstractions.Queue;
using Patents.ArtRepoCloud.Domain.Extensions;
using Patents.ArtRepoCloud.Domain.IntegrationEvents;
using Patents.ArtRepoCloud.Domain.Settings.EventBusSettings;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.ZipPdf
{
    public class CreatePdfZipCommandHandler : IRequestHandler<CreatePdfZipCommand, CreatePdfZipCommandResult>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IDocumentRepository _patentRepository;
        private readonly IDirectorySettings _settings;
        private readonly IEventBusQueueClient<ArtRepoFileServiceBusQueueClientSettings> _busArchiveQueueClient;
        private readonly ILogger<CreatePdfZipCommandHandler> _logger;

        public CreatePdfZipCommandHandler(
            IFileRepository fileRepository, 
            IDocumentRepository patentRepository, 
            IDirectorySettings settings, 
            IEventBusQueueClient<ArtRepoFileServiceBusQueueClientSettings> busArchiveQueueClient, 
            ILogger<CreatePdfZipCommandHandler> logger)
        {
            _fileRepository = fileRepository;
            _patentRepository = patentRepository;
            _settings = settings;
            _busArchiveQueueClient = busArchiveQueueClient;
            _logger = logger;
        }

        public async Task<CreatePdfZipCommandResult> Handle(CreatePdfZipCommand command, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting {nameof(CreatePdfZipCommandHandler)} for References: {string.Join(",", command.ReferenceNumbers)}");

            var documents = await _patentRepository.GetByReferenceNumbersAsync(command.ReferenceNumbers.RemoveNullAndEmptyValues().Distinct(), cancellationToken)
                .ConfigureAwait(false);

            var referenceNumbers = documents.Select(d => d.ReferenceNumber).ToArray();

            var hash = referenceNumbers.CalculateHash();

            var path = _fileRepository.ZipPath(hash);

            var isExist = await _fileRepository.IsExistAsync(path, false, cancellationToken).ConfigureAwait(false);

            if (!isExist)
            {
                await _busArchiveQueueClient.Enqueue(new FilesZipIntegrationEvent(referenceNumbers)).ConfigureAwait(false);
            }

            return new CreatePdfZipCommandResult(hash);
        }
    }
}