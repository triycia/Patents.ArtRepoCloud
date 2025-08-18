using Autofac;
using Patents.ArtRepoCloud.Domain.Extensions;
using Patents.ArtRepoCloud.Domain.IntegrationEvents;
using Patents.ArtRepoCloud.Service.Application.Commands.EnqueuePdf;
using MediatR;

namespace Patents.ArtRepoCloud.Service.Application.Commands.EnqueueImages
{
    class EnqueueImagesCommandHandler : IRequestHandler<EnqueueImagesCommand, EnqueueImagesCommandResult>
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly ILogger<EnqueuePdfCommandHandler> _logger;

        public EnqueueImagesCommandHandler(ILifetimeScope lifetimeScope, ILogger<EnqueuePdfCommandHandler> logger)
        {
            _lifetimeScope = lifetimeScope;
            _logger = logger;
        }

        public async Task<EnqueueImagesCommandResult> Handle(EnqueueImagesCommand command, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting images enqueue process for ReferenceNumber/Priority: {command.ReferenceNumber}/{command.Priority}.");

            try
            {
                var pdfEvt = new ArtRepoImageIntegrationEvent(command.ReferenceNumber, command.Priority, command.ImportSource, command.RetryCount);

                await using var scope = _lifetimeScope.BeginLifetimeScope();

                var queueClient = scope.ResolvePatentQueueClient(command.Priority);

                await queueClient.Enqueue(pdfEvt).ConfigureAwait(false);

                _logger.LogDebug($"Successfully completed images enqueue process for ReferenceNumber/Priority: {command.ReferenceNumber}/{command.Priority}.");

                return new EnqueueImagesCommandResult(true);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Failed images enqueue process for ReferenceNumber/Priority: {command.ReferenceNumber}/{command.Priority}");

                return new EnqueueImagesCommandResult(false);
            }
        }
    }
}