using Autofac;
using Vikcher.Framework.EventBus.Events;
using Patents.ArtRepoCloud.Domain.Code.Interfaces;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.IntegrationEvents;
using Patents.ArtRepoCloud.Domain.Extensions;
using MediatR;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate;
using Patents.ArtRepoCloud.GraphService.Enums;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.EnqueueBatchCommand
{
    public class EnqueueBatchCommandHandler : IRequestHandler<EnqueueBatchCommand, EnqueueBatchCommandResult>
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IReferenceNumberParser _referenceNumberParser;
        private readonly IDocumentRepository _patentRepository;
        private readonly ILogger<EnqueueBatchCommandHandler> _logger;

        public EnqueueBatchCommandHandler(
            ILifetimeScope lifetimeScope,
            IReferenceNumberParser referenceNumberParser,
            IDocumentRepository patentRepository,
            ILogger<EnqueueBatchCommandHandler> logger)
        {
            _lifetimeScope = lifetimeScope;
            _referenceNumberParser = referenceNumberParser;
            _patentRepository = patentRepository;
            _logger = logger;
        }

        public async Task<EnqueueBatchCommandResult> Handle(EnqueueBatchCommand command, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting batch enqueue process, ReferenceNumber/Priority: {string.Join(", ", command.EnqueueBatches.Select(x => $"{x.ReferenceNumber}/{x.Priority}"))}");

            var result = new HashSet<EnqueueBatchCommandResult.ReferenceEnqueueStatus>();

            await using var scope = _lifetimeScope.BeginLifetimeScope();

            foreach (var item in command.EnqueueBatches)
            {
                try
                {
                    var referenceNumber = _referenceNumberParser.Parse(item.ReferenceNumber);

                    if (referenceNumber == null)
                    {
                        _logger.LogWarning($"Unable to parse ReferenceNumber {item.ReferenceNumber}");

                        result.Add(new(item.ReferenceNumber, EnqueueStatus.ParseError));

                        continue;
                    }

                    var document = await _patentRepository
                        .GetByReferenceNumberAsync(referenceNumber.SourceReferenceNumber, cancellationToken)
                        .ConfigureAwait(false);

                    if (document == null)
                    {
                        document = new ArtRepoDocument(referenceNumber.SourceReferenceNumber);

                        _patentRepository.Add(document, cancellationToken);

                        await _patentRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    }
                    else if (!(item.Rescrape ?? false) && document.ImportStatus!.ImportDateTime.Date.Equals(DateTime.Today.Date))
                    {
                        result.Add(new(item.ReferenceNumber, EnqueueStatus.Duplicate));

                        continue;
                    }
                    else if (!(item.Rescrape ?? false))
                    {
                        result.Add(new(item.ReferenceNumber, EnqueueStatus.AlreadyExist));

                        continue;
                    }

                    IntegrationEvent docEvt = command.Source == ImportSource.Uspto
                        ? new PAIRDocumentIntegrationEvent(referenceNumber, item.Priority!.Value)
                        : new ArtRepoDocumentIntegrationEvent(referenceNumber, item.Priority!.Value, command.Source!.Value,
                            command.UseSourceStrictly!.Value, true);

                    var queueClient = scope.ResolveQueueClient(
                        item.Priority!.Value,
                        command.Source.Equals(ImportSource.Uspto)
                            ? ImportSource.Uspto
                            : ImportSource.All);

                    document.SetDocumentImportStatus(QueueStatus.Queued);

                    _patentRepository.Update(document, cancellationToken);

                    await _patentRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    await queueClient.Enqueue(docEvt).ConfigureAwait(false);

                    result.Add(new(item.ReferenceNumber, EnqueueStatus.Success));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Enqueue failed for ReferenceNumber: {item.ReferenceNumber}");

                    result.Add(new(item.ReferenceNumber, EnqueueStatus.InternalServerError));

                    return new EnqueueBatchCommandResult(result.ToArray(), false);
                }
            }

            _logger.LogDebug(
                $"Successfully completed batch enqueue, ReferenceNumber/Priority: {string.Join(", ", command.EnqueueBatches.Select(x => $"{x.ReferenceNumber}/{x.Priority}"))}");

            return new EnqueueBatchCommandResult(result.ToArray());
        }
    }
}