using Patents.ArtRepoCloud.Domain.Interfaces;
using Vikcher.Framework.Common;
using MediatR;

namespace Patents.ArtRepoCloud.Service.Application.Commands.ImportStatus
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

        public async Task<ImportStatusCommandResult> Handle(ImportStatusCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Handling {nameof(ImportStatusCommand)}:{command.ToJson()}.");

            try
            {
                var document = await _patentRepository
               .GetByReferenceNumberAsync(command.ReferenceNumber.SourceReferenceNumber, cancellationToken)
               .ConfigureAwait(false);

                if (document == null)
                {
                    throw new InvalidOperationException(
                        $"{nameof(ImportStatusCommand)} failed. Reference {command.ReferenceNumber.SourceReferenceNumber} not found!");
                }

                document.SetDocumentImportStatus(command.Status);

                _patentRepository.Update(document, cancellationToken);

                await _patentRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                return new ImportStatusCommandResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed {nameof(ImportStatusCommand)}:{command.ToJson()}.");

                return new ImportStatusCommandResult(false);
            }
        }
    }
}