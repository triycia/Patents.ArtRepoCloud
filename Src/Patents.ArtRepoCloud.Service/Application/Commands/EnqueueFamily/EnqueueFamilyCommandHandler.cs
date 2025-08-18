using Autofac;
using Patents.ArtRepoCloud.Domain.Code.Interfaces;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.Extensions;
using Patents.ArtRepoCloud.Domain.IntegrationEvents;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Vikcher.Framework.Data.Cosmos;
using MediatR;

namespace Patents.ArtRepoCloud.Service.Application.Commands.EnqueueFamily
{
    public class EnqueueFamilyCommandHandler : IRequestHandler<EnqueueFamilyCommand, EnqueueFamilyCommandResult>
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IReferenceNumberParser _referenceNumberParser;
        private readonly IDocumentRepository _documentRepository;
        private readonly ILogger<EnqueueFamilyCommandHandler> _logger;

        public EnqueueFamilyCommandHandler(ILifetimeScope lifetimeScope, IReferenceNumberParser referenceNumberParser, IDocumentRepository documentRepository, ILogger<EnqueueFamilyCommandHandler> logger)
        {
            _lifetimeScope = lifetimeScope;
            _referenceNumberParser = referenceNumberParser;
            _documentRepository = documentRepository;
            _logger = logger;
        }

        public async Task<EnqueueFamilyCommandResult> Handle(EnqueueFamilyCommand command, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting {nameof(EnqueueFamilyCommand)} for ReferenceNumber: {command.ReferenceNumber}.");

            var documents = await _documentRepository.QueryDocuments()
                .Where(x => command.FamilyReferenceNumbers.Contains(x.ReferenceNumber))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var existingReferenceNumbers = documents.Select(x => x.ReferenceNumber);

            var newFamilyMembers = command.FamilyReferenceNumbers.Except(existingReferenceNumbers).ToList();

            if (newFamilyMembers.Any())
            {
                foreach (var member in newFamilyMembers)
                {
                    var referenceNumber = _referenceNumberParser.Parse(member);

                    if (referenceNumber == null)
                    {
                        _logger.LogWarning($"Unable to parse family member ReferenceNumber {member}");

                        continue;
                    }

                    var pdfEvt = new ArtRepoDocumentIntegrationEvent(referenceNumber, ImportPriority.Idle, ImportSource.All, false);

                    await using var scope = _lifetimeScope.BeginLifetimeScope();

                    var queueClient = scope.ResolvePatentQueueClient(ImportPriority.Idle);

                    await queueClient.Enqueue(pdfEvt).ConfigureAwait(false);
                }

                _logger.LogDebug($"Successfully completed {nameof(EnqueueFamilyCommand)} for ReferenceNumber: {command.ReferenceNumber}. Family: {string.Join(", ", newFamilyMembers)}");
            }
            else
            {
                _logger.LogDebug($"Successfully completed {nameof(EnqueueFamilyCommand)} for ReferenceNumber: {command.ReferenceNumber}. No new items enqueued.");
            }

            return new EnqueueFamilyCommandResult(true);
        }
    }
}