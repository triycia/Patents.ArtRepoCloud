using Autofac;
using Patents.ArtRepoCloud.Domain.IntegrationEvents;
using Patents.ArtRepoCloud.Domain.Extensions;
using MediatR;
using Vikcher.Framework.EventBus.Events;
using Patents.ArtRepoCloud.Domain.Enums;

namespace Patents.ArtRepoCloud.Service.Application.Commands.EnqueuePdf
{
    internal class EnqueuePdfCommandHandler : IRequestHandler<EnqueuePdfCommand, EnqueuePdfCommandResult>
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly ILogger<EnqueuePdfCommandHandler> _logger;

        public EnqueuePdfCommandHandler(ILifetimeScope lifetimeScope, ILogger<EnqueuePdfCommandHandler> logger)
        {
            _lifetimeScope = lifetimeScope;
            _logger = logger;
        }

        public async Task<EnqueuePdfCommandResult> Handle(EnqueuePdfCommand command, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting pdf enqueue process for ReferenceNumber/Priority: {command.ReferenceNumber}/{command.Priority}.");

            try
            {
                IntegrationEvent pdfEvt = command.ImportSource == ImportSource.Uspto
                    ? new PAIRPdfIntegrationEvent(command.ReferenceNumber, command.Priority, command.RetryCount)
                    : new ArtRepoPdfIntegrationEvent(command.ReferenceNumber, command.Priority, command.ImportSource, command.RetryCount);

                await using var scope = _lifetimeScope.BeginLifetimeScope();

                var queueClient = scope.ResolveQueueClient(command.Priority, command.ImportSource.Equals(ImportSource.Uspto) ? ImportSource.Uspto : ImportSource.All);

                await queueClient.Enqueue(pdfEvt).ConfigureAwait(false);

                _logger.LogDebug($"Successfully completed pdf enqueue process for ReferenceNumber/Priority: {command.ReferenceNumber}/{command.Priority}.");

                return new EnqueuePdfCommandResult(true);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Failed pdf enqueue process for ReferenceNumber/Priority: {command.ReferenceNumber}/{command.Priority}");

                return new EnqueuePdfCommandResult(false);
            }
        }
    }
}