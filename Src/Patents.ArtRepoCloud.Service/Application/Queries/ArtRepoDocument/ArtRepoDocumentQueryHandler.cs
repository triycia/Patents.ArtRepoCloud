using Patents.ArtRepoCloud.Domain.Interfaces;
using MediatR;

namespace Patents.ArtRepoCloud.Service.Application.Queries.ArtRepoDocument
{
    public class ArtRepoDocumentQueryHandler : IRequestHandler<ArtRepoDocumentQuery, ArtRepoDocumentQueryResult>
    {
        private readonly IDocumentRepository _patentRepository;
        private readonly ILogger<ArtRepoDocumentQueryHandler> _logger;

        public ArtRepoDocumentQueryHandler(IDocumentRepository patentRepository, ILogger<ArtRepoDocumentQueryHandler> logger)
        {
            _patentRepository = patentRepository;
            _logger = logger;
        }

        public async Task<ArtRepoDocumentQueryResult> Handle(ArtRepoDocumentQuery query, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Checking if reference number {query.ReferenceNumber} exists in our system.");

            var document = await _patentRepository.GetByReferenceNumberAsync(query.ReferenceNumber.SourceReferenceNumber, cancellationToken)
                .ConfigureAwait(false);

            return new ArtRepoDocumentQueryResult(document);
        }
    }
}