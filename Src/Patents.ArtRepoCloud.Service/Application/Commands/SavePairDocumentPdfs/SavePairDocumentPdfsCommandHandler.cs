using Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.IntegrationEvents;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Patents.ArtRepoCloud.Domain.ValueObjects;
using MediatR;
using static iTextSharp.text.pdf.AcroFields;

namespace Patents.ArtRepoCloud.Service.Application.Commands.SavePairDocumentPdfs
{
    public class SavePairDocumentPdfsCommandHandler : IRequestHandler<SavePairDocumentPdfsCommand, SavePairDocumentPdfsCommandResult>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IDocumentRepository _patentRepository;
        private readonly ILogger<SavePairDocumentPdfsCommandHandler> _logger;

        public SavePairDocumentPdfsCommandHandler(IFileRepository fileRepository, IDocumentRepository patentRepository, ILogger<SavePairDocumentPdfsCommandHandler> logger)
        {
            _fileRepository = fileRepository;
            _patentRepository = patentRepository;
            _logger = logger;
        }

        public async Task<SavePairDocumentPdfsCommandResult> Handle(SavePairDocumentPdfsCommand command, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting {nameof(SavePairDocumentPdfsCommand)} for Reference # {command.ReferenceNumber}");

            try
            {
                var document = await _patentRepository.GetByReferenceNumberAsync(command.ReferenceNumber.SourceReferenceNumber, cancellationToken)
                .ConfigureAwait(false);

                if (document == null)
                {
                    _logger.LogWarning($"{nameof(SavePairDocumentPdfsCommand)} have failed for reference {command.ReferenceNumber}. Document not found in our system!");

                    throw new InvalidOperationException(
                        $"{nameof(ArtRepoImageIntegrationEvent)} have failed for reference {command.ReferenceNumber}. Document not found in our system!");
                }

                var tasks = command.PdfFiles.Select(x => _fileRepository.FinalizeTempFileAsync($"{x.FilePath}/{x.OnDiskId}", cancellationToken));

                await Task.WhenAll(tasks).ConfigureAwait(false);

                var dateTimeNow = DateTime.Now;

                foreach (var file in command.PdfFiles)
                {
                    var pairFile = new PairFileData(
                        file.DocumentCode,
                        file.DocumentDescription,
                        file.Category,
                        file.IFWCheckboxIndex,
                        file.ObjectId,
                        file.PageCount,
                        file.MailRoomDate,
                        dateTimeNow,
                        dateTimeNow
                    );

                    document.PairData?.AddPairFile(pairFile);
                }

                _patentRepository.Update(document);

                await _patentRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogDebug($"Completed {nameof(SavePairDocumentPdfsCommand)} for Reference # {command.ReferenceNumber}");

                return new SavePairDocumentPdfsCommandResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed {nameof(SavePairDocumentPdfsCommand)} for Reference # {command.ReferenceNumber}");

                return new SavePairDocumentPdfsCommandResult(false);
            }
        }
    }
}