using Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.Extensions;
using Patents.ArtRepoCloud.Domain.Interfaces;
using MediatR;

namespace Patents.ArtRepoCloud.Service.Application.Commands.SavePairDocument
{
    public class SavePairDocumentCommandHandler : IRequestHandler<SavePairDocumentCommand, SavePairDocumentCommandResult>
    {
        private readonly IDocumentRepository _patentRepository;
        private readonly ILogger<SavePairDocumentCommandHandler> _logger;

        public SavePairDocumentCommandHandler(IDocumentRepository patentRepository, ILogger<SavePairDocumentCommandHandler> logger)
        {
            _patentRepository = patentRepository;
            _logger = logger;
        }

        public async Task<SavePairDocumentCommandResult> Handle(SavePairDocumentCommand command, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting {nameof(SavePairDocumentCommand)} for Reference # {command.ReferenceNumber}");

            var document = await _patentRepository.GetByReferenceNumberAsync(command.ReferenceNumber.SourceReferenceNumber, cancellationToken)
                .ConfigureAwait(false);

            var dateTimeNow = DateTime.Now;
            var appData = command.ApplicationData;

            if (document == null)
            {
                document = new ArtRepoDocument(
                    command.ReferenceNumber.SourceReferenceNumber,
                    appData.ApplicationNumber, 
                    appData.PublicationNumber, 
                    appData.PatentNumber, 
                    null,
                    command.ReferenceNumber.NumberType.GetDocumentType(),
                    null,
                    command.ReferenceNumber.CountryCode,
                    null,
                    null,
                    null,
                    appData.EffectiveFilingDate,
                    appData.PublicationDate,
                    appData.FilingDate,
                    appData.GrantDate,
                    null,
                    null,
                    null,
                    null,
                    DataSource.Uspto,
                    null,
                    dateTimeNow
                );
            }
            else
            {
                document.SetApplicationNumber(appData.ApplicationNumber);
                document.SetPublicationNumber(appData.PublicationNumber);
                document.SetPatentNumber(appData.PatentNumber);

                document.SetApplicationDate(appData.EffectiveFilingDate);
                document.SetPublicationDate(appData.PublicationDate);
                document.SetDateFiled(appData.FilingDate);

                document.SetDocumentDataSource(DataSource.Uspto);
            }

            document.InitPairData();

            document.PairData.SetTitleOfInvention(appData.TitleOfInvention);
            document.PairData.SetApplicationType(appData.ApplicationType);
            document.PairData.SetExaminerName(appData.ExaminerName);
            document.PairData.SetGroupArtUnit(appData.GroupArtUnit);
            document.PairData.SetConfirmationNumber(appData.ConfirmationNumber);
            document.PairData.SetAttorneyDocketNumber(appData.AttorneyDocketNumber);
            document.PairData.SetClassSubClass(appData.Subclass);
            document.PairData.SetFirstNamedInventor(appData.FirstNamedInventor);
            document.PairData.SetCustomerNumber(appData.CustomerNumber);
            document.PairData.SetStatus(appData.Status);
            document.PairData.SetStatusDate(appData.StatusDate);
            document.PairData.SetLocation(appData.Location);
            document.PairData.SetLocationDate(appData.LocationDate);
            document.PairData.SetEarliestPublicationNumber(appData.EarliestPublicationNumber);
            document.PairData.SetEarliestPublicationDate(appData.EarliestPublicationDate);
            
            if (document.Id.Equals(Guid.Empty))
            {
                _patentRepository.Add(document, cancellationToken);
            }
            else
            {
                document.DocumentMetadata.SetModified();

                _patentRepository.Update(document, cancellationToken);
            }

            await _patentRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new SavePairDocumentCommandResult();
        }
    }
}