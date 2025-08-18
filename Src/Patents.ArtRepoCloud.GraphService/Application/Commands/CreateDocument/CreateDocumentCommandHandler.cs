using System.Text;
using Patents.ArtRepoCloud.Domain.Aggregates.CompanyAggregate.Enums;
using Patents.ArtRepoCloud.Domain.Aggregates.CompanyAggregate;
using Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.Extensions;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Patents.ArtRepoCloud.Domain.ValueObjects;
using Vikcher.Framework.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PDFtoImage;
using SkiaSharp;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.CreateDocument
{
    public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, CreateDocumentCommandResult>
    {
        private readonly IDocumentRepository _patentRepository;
        private readonly IFileRepository _fileRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<CreateDocumentCommandHandler> _logger;

        public CreateDocumentCommandHandler(IDocumentRepository patentRepository, IFileRepository fileRepository, ICompanyRepository companyRepository, ILogger<CreateDocumentCommandHandler> logger)
        {
            _patentRepository = patentRepository;
            _fileRepository = fileRepository;
            _companyRepository = companyRepository;
            _logger = logger;
        }

        public async Task<CreateDocumentCommandResult> Handle(CreateDocumentCommand command, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting {nameof(CreateDocumentCommand)} for {command.ReferenceNumber}");

            var doc = command;

            var dateCreated = DateTime.Now;

            var document = new ArtRepoDocument(
                doc.ReferenceNumber,
                doc.ApplicationNumber ?? string.Empty, 
                null, 
                null, 
                null,
                doc.DocumentType,
                null,
                doc.Country ?? string.Empty,
                doc.TermAdjustment,
                doc.EarliestPriorityNumber,
                doc.EarliestPriorityDate,
                doc.ApplicationDate,
                doc.PublicationDate,
                doc.DateFiled,
                doc.IssueDate,
                doc.ExpirationDate,
                doc.OriginalExpirationDate,
                doc.AdjustedExpirationDate,
                doc.PctFilingDate,
                DataSource.Uploaded,
                doc.SourceReferenceNumber,
                dateCreated
            );

            var documentData = new DocumentData(
                null, 
                doc.AssigneeName, 
                null,
                doc.DocumentClassifications?.Select(c => new DocumentClassification(
                    c.Classification, 
                    c.ClassificationType))
                    .ToList() ?? new List<DocumentClassification>(),
                doc.DocumentTranslations?.Select(t => new DocumentTranslation(
                    t.IsOriginalLanguage,
                    false,
                    t.Language,
                    t.Title,
                    t.Abstract,
                    t.Description,
                    dateCreated,
                    t.Claims.Select(c =>
                        new Claim(
                            c.Number,
                            c.Text,
                            c.IsIndependent,
                            c.IsCanceled)).ToList()))
                    .ToList() ?? new List<DocumentTranslation>(),
                null,
                doc.Notes);

            string json = JsonConvert.SerializeObject(documentData.ToJson());

            var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            var relativeBlobPath = $"{document.ReferenceNumber}/".ToLower();
            var newFilePath = _fileRepository.BibDataPath(relativeBlobPath);

            var onDiskGuid = await _fileRepository
                .SaveAsync(jsonStream, newFilePath, cancellationToken)
                .ConfigureAwait(false);

            document.SetDocumentDataFile($"{relativeBlobPath}{onDiskGuid}");

            _patentRepository.Add(document, cancellationToken);

            await _patentRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(command.AssigneeName))
            {
                await BuildCompanyAsync(command.ReferenceNumber, command.AssigneeName, cancellationToken).ConfigureAwait(false);
            }

            if (command.File != null)
            {
                var pdfFilePath = $"{command.File.BlobPath}{command.File.OnDiskGuid}";

                var isExist = await _fileRepository.IsExistAsync(pdfFilePath, false, cancellationToken).ConfigureAwait(false);

                if (isExist)
                {
                    var fileStream = await _fileRepository.GetAsync(pdfFilePath, cancellationToken).ConfigureAwait(false);

                    if (fileStream.IsPdfValid(out int pageCount, out int length))
                    {
                        var relativePdfBlobPath = $"{doc.ReferenceNumber}/pdf/".ToLower();
                        var path = _fileRepository.RootPath(relativePdfBlobPath);

                        await _fileRepository.SaveAsync(fileStream!, path, command.File.OnDiskGuid.ToString(), cancellationToken).ConfigureAwait(false);

                        document.SetDocumentFile(new DocumentFile(
                            command.File.OnDiskGuid,
                            command.File.FileName,
                            MediaTypes.Pdf,
                            relativePdfBlobPath,
                            pageCount,
                            length,
                            DataSource.Uploaded,
                            dateCreated));

                        _patentRepository.Update(document, cancellationToken);

                        await _patentRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                        _logger.LogDebug($"Saved document file for {command.ReferenceNumber} of media type {MediaTypes.Pdf}.");
                    }
                    else
                    {
                        _logger.LogWarning($"Corrupted PDF file identified for {command.ReferenceNumber}.");
                    }
                }

                if (command.DesignatedImages.Any())
                {
                    var relativeImgBlobPath = $"{doc.ReferenceNumber}/img/".ToLower();

                    int sequence = document.DocumentImages.Select(img => img.Sequence).OrderBy(x => x).LastOrDefault();

                    var firstImgPath = $"{command.TemporaryFilesPath}{command.DesignatedImages.First()}.jpeg";
                    var isFirstImgExist = await _fileRepository.IsExistAsync(firstImgPath, false, cancellationToken).ConfigureAwait(false);

                    if (isFirstImgExist)
                    {
                        foreach (var designatedNumber in command.DesignatedImages)
                        {
                            var imgPath = $"{command.TemporaryFilesPath}{designatedNumber}.jpeg";

                            var isImgExist = await _fileRepository.IsExistAsync(imgPath, false, cancellationToken).ConfigureAwait(false);
                            var imgStream = isImgExist
                                ? await _fileRepository.GetAsync(imgPath, cancellationToken).ConfigureAwait(false)
                                : null;

                            if (imgStream != null)
                            {
                                var length = imgStream.Length;
                                var path = _fileRepository.RootPath(relativeImgBlobPath);

                                var imgGuid = await _fileRepository.SaveAsync(
                                    imgStream,
                                    path,
                                    cancellationToken);

                                sequence++;

                                document.AddDocumentImage(new DocumentImage(
                                    imgGuid,
                                    $"{document.ReferenceNumber}_{designatedNumber}.jpeg",
                                    MediaTypes.Jpeg,
                                    relativeImgBlobPath,
                                    length,
                                    DataSource.Uploaded,
                                    dateCreated,
                                    sequence,
                                    designatedNumber));

                                _patentRepository.Update(document, cancellationToken);

                                await _patentRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                                await _fileRepository.DeleteAsync(imgPath, cancellationToken).ConfigureAwait(false);

                                _logger.LogDebug($"Saved document designated images for {command.ReferenceNumber} of media type {MediaTypes.Jpeg}.");
                            }
                        }
                    }
                    else
                    {
                        var fileStream = await _fileRepository.GetAsync(pdfFilePath, cancellationToken).ConfigureAwait(false);

                        if (fileStream != null)
                        {
                            var images = Conversion.ToImages(fileStream).ToList();

                            var pageNumber = 0;
                            foreach (var image in images)
                            {
                                pageNumber++;
                                if (command.DesignatedImages?.Contains(pageNumber) ?? true)
                                {
                                    using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 1))
                                    using (MemoryStream imgStream = new MemoryStream())
                                    {
                                        data.SaveTo(imgStream);

                                        imgStream.Position = 0;

                                        var imgLength = imgStream.Length;
                                        var path = _fileRepository.RootPath(relativeImgBlobPath);

                                        var imgGuid = await _fileRepository.SaveAsync(
                                            imgStream,
                                            path,
                                            cancellationToken);

                                        sequence++;

                                        document.AddDocumentImage(new DocumentImage(
                                            imgGuid,
                                            $"{document.ReferenceNumber}_{pageNumber}.jpeg",
                                            MediaTypes.Jpeg,
                                            relativeImgBlobPath,
                                            imgLength,
                                            DataSource.Uploaded,
                                            dateCreated,
                                            sequence,
                                            pageNumber));

                                        _patentRepository.Update(document, cancellationToken);

                                        await _patentRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                                        _logger.LogDebug($"Saved document designated images for {command.ReferenceNumber} of media type {MediaTypes.Jpeg}.");
                                    }

                                    image.Dispose();
                                }
                            }
                        }
                    }
                }
            }

            return new CreateDocumentCommandResult(document.ReferenceNumber, true);
        }

        private async Task BuildCompanyAsync(string referenceNumber, string assigneeName, CancellationToken cancellationToken)
        {
            var company = await _companyRepository.CompaniesQuery()
                .FirstOrDefaultAsync(c => c.CompanyName == assigneeName, cancellationToken)
                .ConfigureAwait(false);

            if (company == null)
            {
                company = new Company(assigneeName, CompanyReviewStatus.New, null);

                await _companyRepository.AddCompanyAsync(company, cancellationToken).ConfigureAwait(false);

                await _companyRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation(
                    $"Created new Company: Id/CompanyName {company.Id}/{company.CompanyName} for the ReferenceNumber: {referenceNumber}.");
            }

            var companyDocument = await _companyRepository.CompanyDocumentsQuery()
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ReferenceNumber == referenceNumber, cancellationToken)
                .ConfigureAwait(false);

            if (companyDocument != null)
            {
                _logger.LogDebug(
                    $"Company with Id: {companyDocument.CompanyId} found for the ReferenceNumber: {referenceNumber}.");
                return;
            }

            await _companyRepository
                .AddCompanyDocumentAsync(new CompanyDocument(referenceNumber, company.Id),
                    cancellationToken)
                .ConfigureAwait(false);

            await _companyRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogDebug($"Successfully completed Company build for the ReferenceNumber: {referenceNumber}.");
        }
    }
}