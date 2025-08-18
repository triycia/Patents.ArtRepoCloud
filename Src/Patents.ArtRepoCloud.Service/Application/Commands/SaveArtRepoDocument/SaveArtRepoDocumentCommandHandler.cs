using System.Text;
using Patents.ArtRepoCloud.Domain.Aggregates.CompanyAggregate;
using Patents.ArtRepoCloud.Domain.Aggregates.CompanyAggregate.Enums;
using Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.Extensions;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Domain.ValueObjects;
using Vikcher.Framework.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Patents.ArtRepoCloud.Service.Application.Commands.SaveArtRepoDocument
{
    public class SaveArtRepoDocumentCommandHandler : IRequestHandler<SaveArtRepoDocumentCommand, SaveArtRepoDocumentCommandResult>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IDocumentRepository _patentRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<SaveArtRepoDocumentCommandHandler> _logger;

        public SaveArtRepoDocumentCommandHandler(
            IFileRepository fileRepository,
            IDocumentRepository patentRepository,
            ICompanyRepository companyRepository,
            ILogger<SaveArtRepoDocumentCommandHandler> logger)
        {
            _fileRepository = fileRepository;
            _patentRepository = patentRepository;
            _companyRepository = companyRepository;
            _logger = logger;
        }

        public async Task<SaveArtRepoDocumentCommandResult> Handle(SaveArtRepoDocumentCommand command, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting {nameof(SaveArtRepoDocumentCommand)} for Reference # {command.ReferenceNumber}");

            var document = await _patentRepository.GetByReferenceNumberAsync(command.ReferenceNumber.SourceReferenceNumber, cancellationToken)
                .ConfigureAwait(false);

            var dateTimeNow = DateTime.Now;
            var bib = command.Document;

            var dataSourceReferenceNumber = command.IsUserImport
                ? bib.DocumentNumber?.ToString() ?? command.ReferenceNumber.ToString()
                : null;

            if (document == null)
            {
                _logger.LogDebug($"Creating new document for {command.ReferenceNumber}.");


                document = new ArtRepoDocument(
                    command.ReferenceNumber.SourceReferenceNumber,
                    bib.ApplicationNumber,
                    null,
                    null,
                    null,
                    command.ReferenceNumber.NumberType.GetDocumentType(),
                    bib.FamilyId,
                    bib.Country,
                    bib.TermAdjustment,
                    bib.EarliestPriorityNumber,
                    bib.EarliestPriorityDate,
                    bib.ApplicationDate,
                    bib.PublicationDate,
                    bib.DateFiled,
                    null,
                    bib.ExpirationDate,
                    bib.OriginalExpirationDate,
                    bib.AdjustedExpirationDate,
                    null,
                    command.DataSource,
                    dataSourceReferenceNumber,
                    dateTimeNow
                );

                if (bib.FamilySimpleList.Any())
                {
                    bib.FamilySimpleList.ToList().ForEach(document.AddFamilySimples);
                }

                if (bib.FamilyExtendedList.Any())
                {
                    bib.FamilyExtendedList.ToList().ForEach(document.AddFamilyExtendeds);
                }
            }
            else
            {
                document.InitDocument(
                    command.ReferenceNumber.NumberType.GetDocumentType(),
                    bib.FamilyId,
                    bib.Country,
                    bib.TermAdjustment,
                    bib.EarliestPriorityNumber,
                    bib.EarliestPriorityDate,
                    bib.ApplicationDate,
                    bib.PublicationDate,
                    bib.DateFiled,
                    null,
                    bib.ExpirationDate,
                    bib.OriginalExpirationDate,
                    bib.AdjustedExpirationDate,
                    null);

                document.SetTermAdjustment(bib.TermAdjustment);
                document.SetEarliestPriorityNumber(bib.EarliestPriorityNumber);
                document.SetEarliestPriorityDate(bib.EarliestPriorityDate);
                document.SetApplicationDate(bib.ApplicationDate);
                document.SetPublicationDate(bib.PublicationDate);
                document.SetDateFiled(bib.DateFiled);
                document.SetExpirationDate(bib.ExpirationDate);
                document.SetOriginalExpirationDate(bib.OriginalExpirationDate);
                document.SetAdjustedExpirationDate(bib.AdjustedExpirationDate);
                document.SetApplicationNumber(bib.ApplicationNumber.Replace("\\", "")
                    .Replace("-", ""));

                if (bib.FamilySimpleList.Any())
                {
                    document.FamilySimples.Clear();

                    bib.FamilySimpleList.ToList().ForEach(document.AddFamilySimples);
                }

                if (bib.FamilyExtendedList.Any())
                {
                    document.FamilyExtendeds.Clear();

                    bib.FamilyExtendedList.ToList().ForEach(document.AddFamilyExtendeds);
                }

                document.DocumentMetadata.SetModified();

                document.SetDocumentDataSource(command.DataSource, dataSourceReferenceNumber);
            }

            DocumentData? documentData = null;

            var blobDataPath = document.DocumentDataBlobPath != null ? _fileRepository.BibDataPath(document.DocumentDataBlobPath!) : null;

            if (blobDataPath != null && await IsExist(blobDataPath).ConfigureAwait(false))
            {
                var documentDataStream = await _fileRepository.GetAsync(blobDataPath, cancellationToken).ConfigureAwait(false);

                documentData = documentDataStream.ReadAs<DocumentData>();

                if (!documentData?.AssigneeName.Compare(bib.AssigneeName) ?? false)
                {
                    documentData.SetAssigneeName(bib.AssigneeName);
                }

                if (!documentData?.Authors.Compare(bib.Authors) ?? false)
                {
                    documentData.SetAuthors(bib.Authors);
                }

                if (!documentData?.TerminalDisclaimer.Compare(bib.TerminalDisclaimer) ?? false)
                {
                    documentData.SetTerminalDisclaimer(bib.TerminalDisclaimer);
                }
            }

            documentData ??= new DocumentData(bib.TerminalDisclaimer, bib.AssigneeName, bib.Authors);

            if (document.DocumentMetadata.DateCreated.Equals(dateTimeNow))
            {
                _patentRepository.Add(document, cancellationToken);
            }
            else
            {
                _patentRepository.Update(document, cancellationToken);
            }

            await _patentRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            BuildDocumentTranslations(command, documentData);
            BuildDocumentClassifications(command, documentData);

            var jsonStr = JsonConvert.SerializeObject(documentData.ToJson());
            var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonStr));
            var relativeBlobPath = $"{document.ReferenceNumber}/".ToLower();
            var newFilePath = _fileRepository.BibDataPath(relativeBlobPath);

            var onDiskGuid = await _fileRepository
                .SaveAsync(jsonStream, newFilePath, cancellationToken)
                .ConfigureAwait(false);

            if (blobDataPath != null && await IsExist(blobDataPath).ConfigureAwait(false))
            {
                await _fileRepository.DeleteAsync(blobDataPath, cancellationToken).ConfigureAwait(false);
            }

            document.SetDocumentDataFile($"{relativeBlobPath}{onDiskGuid}");

            await BuildCompanyAsync(command.ReferenceNumber, bib.AssigneeName, cancellationToken).ConfigureAwait(false);

            document.SetDocumentImportStatus(QueueStatus.Completed);

            _patentRepository.Update(document, cancellationToken);

            await _patentRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            Task<bool> IsExist(string path) => _fileRepository.IsExistAsync(path, false, cancellationToken);

            return new SaveArtRepoDocumentCommandResult(document);
        }

        private void BuildDocumentTranslations(SaveArtRepoDocumentCommand command, DocumentData documentData)
        {
            if (!(command.Document.Translations.Any()))
            {
                _logger.LogInformation($"No document translation found for {command.ReferenceNumber}.");
                return;
            }

            if (documentData.DocumentTranslations.Any())
            {
                documentData.DocumentTranslations.Clear();
            }
            
            if (command.Document.Translations.Any())
            {
                foreach (var t in command.Document.Translations)
                {
                    documentData.AddDocumentTranslation(new DocumentTranslation(
                        t.IsOriginalLanguage,
                        t.IsDefault,
                        t.Language,
                        t.Title,
                        t.Abstract,
                        t.Description,
                        DateTime.Now,
                        t.Claims.Select(c =>
                                new Claim(
                                    c.Number,
                                    c.Text,
                                    c.IsIndependent,
                                    c.IsCanceled))
                            .ToList()));
                }
            }

            var defaultTranslation = documentData.DocumentTranslations.FirstOrDefault(dt => !string.IsNullOrWhiteSpace(dt.Language) &&
                                                                       dt.Language.Equals(Languages.English,
                                                                           StringComparison.CurrentCultureIgnoreCase))
                                     ?? documentData.DocumentTranslations.FirstOrDefault(dt => dt.IsOriginalLanguage);

            documentData.DocumentTranslations.ToList().ForEach(t => t.SetIsDefault(defaultTranslation == t));

            _logger.LogDebug($"Transformed document translations for the request {command.ReferenceNumber}.");
        }

        private void BuildDocumentClassifications(SaveArtRepoDocumentCommand command, DocumentData documentData)
        {
            if (documentData.DocumentClassifications.Any())
            {
                documentData.DocumentClassifications.Clear();
            }

            if (command.Document.Classifications?.Any() ?? false)
            {
                foreach (var c in command.Document.Classifications)
                {
                    documentData.DocumentClassifications.Add(new DocumentClassification(c.Text, c.Type));
                }

                _logger.LogDebug($"Transformed document classifications for {command.ReferenceNumber}.");
            }
            else
            {
                _logger.LogInformation($"No document classification found for {command.ReferenceNumber}.");
            }
        }

        private async Task BuildCompanyAsync(ReferenceNumber referenceNumber, string assigneeName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(assigneeName))
            {
                _logger.LogInformation($"No assignee name found for the ReferenceNumber: {referenceNumber}.");
                return;
            }

            try
            {
                var companyDocument = await _companyRepository.CompanyDocumentsQuery()
                    .AsNoTracking()
                    .SingleOrDefaultAsync(x => x.ReferenceNumber == referenceNumber.SourceReferenceNumber, cancellationToken)
                    .ConfigureAwait(false);

                if (companyDocument != null)
                {
                    _logger.LogDebug($"Company with Id: {companyDocument.CompanyId} found for the ReferenceNumber: {referenceNumber}.");
                    return;
                }

                var company = await _companyRepository.CompaniesQuery()
                    .FirstOrDefaultAsync(c => c.CompanyName != null && c.CompanyName.Trim().ToLower() == assigneeName.Trim().ToLower(), cancellationToken)
                    .ConfigureAwait(false);

                if (company == null)
                {
                    company = new Company(assigneeName, CompanyReviewStatus.New, null);

                    await _companyRepository.AddCompanyAsync(company, cancellationToken).ConfigureAwait(false);

                    await _companyRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    _logger.LogInformation(
                        $"Created new Company: Id/CompanyName {company.Id}/{company.CompanyName} for the ReferenceNumber: {referenceNumber}.");
                }

                await _companyRepository
                    .AddCompanyDocumentAsync(new CompanyDocument(referenceNumber.SourceReferenceNumber, company.Id),
                        cancellationToken)
                    .ConfigureAwait(false);

                await _companyRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogDebug($"Successfully completed Company build for the ReferenceNumber: {referenceNumber}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create Company for the ReferenceNumber: {referenceNumber}.");
            }
        }
    }
}