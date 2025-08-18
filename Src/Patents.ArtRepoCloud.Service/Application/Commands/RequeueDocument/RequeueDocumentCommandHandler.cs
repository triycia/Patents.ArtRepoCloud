using Autofac;
using Vikcher.Framework.EventBus.Events;
using Patents.ArtRepoCloud.Domain.Code.Interfaces;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.Extensions;
using Patents.ArtRepoCloud.Domain.IntegrationEvents;
using MediatR;

namespace Patents.ArtRepoCloud.Service.Application.Commands.RequeueDocument
{
    public class RequeueDocumentCommandHandler : IRequestHandler<RequeueDocumentCommand, RequeueDocumentCommandResult>
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IReferenceNumberParser _referenceNumberParser;
        private readonly ILogger<RequeueDocumentCommandHandler> _logger;

        public RequeueDocumentCommandHandler(ILifetimeScope lifetimeScope, IReferenceNumberParser referenceNumberParser, ILogger<RequeueDocumentCommandHandler> logger)
        {
            _lifetimeScope = lifetimeScope;
            _referenceNumberParser = referenceNumberParser;
            _logger = logger;
        }

        public async Task<RequeueDocumentCommandResult> Handle(RequeueDocumentCommand command, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting enqueue process for ReferenceNumber/Priority: {command.ReferenceNumber}/{command.Priority}.");

            try
            {
                IntegrationEvent docEvt = command.Source == ImportSource.Uspto
                    ? new PAIRDocumentIntegrationEvent(
                        command.ReferenceNumber,
                        command.Priority,
                        command.RetryCount)
                    : new ArtRepoDocumentIntegrationEvent(
                        command.ReferenceNumber,
                        command.Priority,
                        command.Source,
                        command.UseSourceStrictly,
                        command.IsUserImport,
                        command.RetryCount);

                await using var scope = _lifetimeScope.BeginLifetimeScope();

                var queueClient = scope.ResolveQueueClient(command.Priority, command.Source.Equals(ImportSource.Uspto) ? ImportSource.Uspto : ImportSource.All);

                await queueClient.Enqueue(docEvt).ConfigureAwait(false);

                _logger.LogDebug($"Successfully completed enqueue process for ReferenceNumber/Priority: {command.ReferenceNumber}/{command.Priority}.");

                return new RequeueDocumentCommandResult(true, command.RetryCount);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Failed to Enqueue ReferenceNumber/Priority: {command.ReferenceNumber}/{command.Priority}");

                return new RequeueDocumentCommandResult(false);
            }
        }
    }
}