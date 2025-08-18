using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.Interfaces;
using MediatR;
using static Patents.ArtRepoCloud.GraphService.Application.Queries.ImportStatus.ImportStatusCommandResult;

namespace Patents.ArtRepoCloud.GraphService.Application.Queries.ImportStatus
{
    public class ImportStatusCommandHandler : IRequestHandler<ImportStatusCommand, ImportStatusCommandResult>
    {
        private readonly IDocumentRepository _patentRepository;
        private readonly ILogger<ImportStatusCommandHandler> _logger;

        public ImportStatusCommandHandler(IDocumentRepository patentRepository, ILogger<ImportStatusCommandHandler> logger)
        {
            _patentRepository = patentRepository;
            _logger = logger;
        }

        public async Task<ImportStatusCommandResult> Handle(ImportStatusCommand query, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting {nameof(ImportStatusCommand)} for ReferenceNumbers: {string.Join(", ", query.ReferenceNumbers)}");

            var referenceStatuses = new List<ImportStatusCommandResult.ReferenceImportStatus>();

            var documents = await _patentRepository
                .GetByReferenceNumbersAsync(query.ReferenceNumbers, cancellationToken)
                .ConfigureAwait(false);

            foreach (var document in documents.Where(d => !d.DocumentMetadata.DocumentDataSource.Equals(DataSource.Uploaded)))
            {
                referenceStatuses.Add(new ReferenceImportStatus(document.ReferenceNumber, document?.ImportStatus!.Status ?? QueueStatus.None));
            }

            return new ImportStatusCommandResult(referenceStatuses);
        }
    }
}