using Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate;
using Patents.ArtRepoCloud.Domain.Code.Interfaces;
using Patents.ArtRepoCloud.Domain.Extensions;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Vikcher.Framework.Common;
using MediatR;
using Newtonsoft.Json;
using System.Text;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ValueObjects;
using PDFtoImage;
using SkiaSharp;
using Patents.ArtRepoCloud.Domain.Aggregates.CompanyAggregate.Enums;
using Patents.ArtRepoCloud.Domain.Aggregates.CompanyAggregate;
using Microsoft.EntityFrameworkCore;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.UpdateDocument
{
    public class UpdateDocumentCommandHandler : IRequestHandler<UpdateDocumentCommand, UpdateDocumentCommandResult>
    {
        private readonly IDocumentRepository _patentRepository;
        private readonly IFileRepository _fileRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<UpdateDocumentCommandHandler> _logger;

        public UpdateDocumentCommandHandler(IDocumentRepository patentRepository, IFileRepository fileRepository, ICompanyRepository companyRepository, ILogger<UpdateDocumentCommandHandler> logger)
        {
            _patentRepository = patentRepository;
            _fileRepository = fileRepository;
            _companyRepository = companyRepository;
            _logger = logger;
        }

        public async Task<UpdateDocumentCommandResult> Handle(UpdateDocumentCommand command, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting {nameof(UpdateDocumentCommand)}: {command.ToJson()}");

            var document = await _patentRepository
                .GetByReferenceNumberAsync(command.ReferenceNumber, cancellationToken)
            .ConfigureAwait(false);

            if (document != null)
            {
                var dateCreated = DateTime.Now;

                document.SetDocumentType(command.DocumentType);
                document.SetPublicationDate(command.PublicationDate);
                document.SetExpirationDate(command.ExpirationDate);

                var blobDataPath = document.DocumentDataBlobPath != null ? _fileRepository.BibDataPath(document.DocumentDataBlobPath!) : null;
                DocumentData? documentData = null;

                if (blobDataPath != null)
                {
                    var documentDataStream = await _fileRepository.GetAsync(blobDataPath, cancellationToken).ConfigureAwait(false);

                    documentData = documentDataStream.ReadAs<DocumentData>();
                }

                if (documentData != null)
                {
                    documentData.SetAssigneeName(command.AssigneeName);

                    var trans = command.DocumentTranslation;

                    if (trans.IsDefault)
                    {
                        foreach (var translation in documentData.DocumentTranslations)
                        {
                            translation.SetIsDefault(false);
                        }
                    }

                    documentData.AddDocumentTranslation(new DocumentTranslation(
                        trans.IsOriginalLanguage,
                        trans.IsDefault,
                        trans.Language,
                        trans.Title,
                        trans.Abstract,
                        trans.Description,
                        dateCreated,
                        trans.Claims.Select(c => new Claim(c.Number, c.Text, c.IsIndependent, c.IsCanceled)).ToList(),
                        Guid.NewGuid()));

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
                }

                _patentRepository.Update(document, cancellationToken);

                await _patentRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(command.AssigneeName))
                {
                    await BuildCompanyAsync(command.ReferenceNumber, command.AssigneeName, cancellationToken).ConfigureAwait(false);
                }

                var pdfFilePath = document.DocumentFile != null
                    ? $"{document.DocumentFile.BlobPath}{document.DocumentFile.Guid}"
                    : null;

                if (command.File != null)
                {
                    pdfFilePath = $"{command.File.BlobPath}{command.File.OnDiskGuid}";

                    var isExist = await _fileRepository.IsExistAsync(pdfFilePath, false, cancellationToken).ConfigureAwait(false);

                    if (isExist)
                    {
                        var fileStream = await _fileRepository.GetAsync(pdfFilePath, cancellationToken).ConfigureAwait(false);

                        if (fileStream.IsPdfValid(out int pageCount, out int length))
                        {
                            var relativePdfBlobPath = $"{document.ReferenceNumber}/pdf/".ToLower();
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
                }

                if (command.DesignatedImages.Any())
                {
                    var relativeImgBlobPath = $"{document.ReferenceNumber}/img/".ToLower();

                    var firstImgPath = $"{command.TemporaryFilesPath}{command.DesignatedImages.First()}.jpeg";
                    var isFirstImgExist = await _fileRepository.IsExistAsync(firstImgPath, false, cancellationToken).ConfigureAwait(false);

                    var designatedImages = command.DesignatedImages ?? Array.Empty<int>();
                    var imagesToDelete =
                        document.DocumentImages.Where(x => !designatedImages.Contains(x.Sequence)).ToList();

                    var existingImagesSequence = document.DocumentImages.Select(x => x.Sequence);

                    designatedImages =
                        designatedImages.Where(x => !existingImagesSequence.Contains(x)).ToArray();

                    if (imagesToDelete.Any())
                    {
                        foreach (var documentImage in imagesToDelete)
                        {
                            document.RemoveDocumentImage(documentImage);
                        }

                        _patentRepository.Update(document, cancellationToken);
                    }

                    if (isFirstImgExist)
                    {
                        foreach (var designatedNumber in designatedImages)
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

                                document.AddDocumentImage(new DocumentImage(
                                    imgGuid,
                                    $"{document.ReferenceNumber}_{designatedNumber}.jpeg",
                                    MediaTypes.Jpeg,
                                    relativeImgBlobPath,
                                    length,
                                    DataSource.Uploaded,
                                    dateCreated,
                                    designatedNumber,
                                    designatedNumber));

                                _patentRepository.Update(document, cancellationToken);

                                await _patentRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                                await _fileRepository.DeleteAsync(imgPath, cancellationToken).ConfigureAwait(false);

                                _logger.LogDebug($"Saved document designated images for {command.ReferenceNumber} of media type {MediaTypes.Jpeg}.");
                            }
                        }
                    }
                    else if(!string.IsNullOrEmpty(pdfFilePath))
                    {
                        var fileStream = await _fileRepository.GetAsync(pdfFilePath, cancellationToken).ConfigureAwait(false);

                        if (fileStream != null)
                        {
                            var images = Conversion.ToImages(fileStream).ToList();

                            var pageNumber = 0;
                            foreach (var image in images)
                            {
                                pageNumber++;
                                if (designatedImages?.Contains(pageNumber) ?? true)
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

                                        document.AddDocumentImage(new DocumentImage(
                                            imgGuid,
                                            $"{document.ReferenceNumber}_{pageNumber}.jpeg",
                                            MediaTypes.Jpeg,
                                            relativeImgBlobPath,
                                            imgLength,
                                            DataSource.Uploaded,
                                            dateCreated,
                                            pageNumber,
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

                Task<bool> IsExist(string path) => _fileRepository.IsExistAsync(path, false, cancellationToken);

                return new UpdateDocumentCommandResult(true);
            }

            return new UpdateDocumentCommandResult(false);
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