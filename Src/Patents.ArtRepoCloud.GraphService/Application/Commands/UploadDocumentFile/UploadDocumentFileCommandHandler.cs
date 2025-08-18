using Patents.ArtRepoCloud.GraphService.Extensions;
using Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.Extensions;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Patents.ArtRepoCloud.Domain.ValueObjects;
using Patents.ArtRepoCloud.Infrastructure;
using Vikcher.Framework.Data.Cosmos;
using MediatR;
using PDFtoImage;
using SkiaSharp;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.UploadDocumentFile
{
    public class UploadDocumentFileCommandHandler : IRequestHandler<UploadDocumentFileCommand, UploadDocumentFileCommandResult>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IDocumentRepository _patentRepository;
        private readonly IUnitOfWork<DocumentDbContext> _unitOfWork;
        private readonly ILogger<UploadDocumentFileCommandHandler> _logger;

        public UploadDocumentFileCommandHandler(
            IFileRepository fileRepository, 
            IDocumentRepository patentRepository, 
            IUnitOfWork<DocumentDbContext> unitOfWork, 
            ILogger<UploadDocumentFileCommandHandler> logger)
        {
            _fileRepository = fileRepository;
            _patentRepository = patentRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<UploadDocumentFileCommandResult> Handle(UploadDocumentFileCommand command, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting {nameof(UploadDocumentFileCommand)} for Reference {command.ReferenceNumber}");

            try
            {
                var document = await _patentRepository.GetByReferenceNumberAsync(command.ReferenceNumber, cancellationToken)
                    .ConfigureAwait(false);

                if (document != null && command.File.IsPdfValid(out int pageCount, out int length))
                {
                    var relativeBlobPath = $"{document.ReferenceNumber.ToLower()}/{DataSource.Uploaded.GetName()}/Pdf/";
                    var newFilePath = _fileRepository.BibDataPath(relativeBlobPath);
                    var dateCreated = DateTime.Now;

                    using (MemoryStream pdfStream = new MemoryStream())
                    {
                        await command.File.CopyToAsync(pdfStream, cancellationToken).ConfigureAwait(false);

                        var onDiskGuid = await _fileRepository
                            .SaveAsync(pdfStream, newFilePath, cancellationToken)
                            .ConfigureAwait(false);

                        if (document.DocumentFile?.BlobPath != null)
                        {
                            var oldFilePath = _fileRepository.RootPath($"{document.DocumentFile.BlobPath}{document.DocumentFile!.Guid}");

                            await _fileRepository.DeleteAsync(oldFilePath, cancellationToken).ConfigureAwait(false);
                        }

                        document.SetDocumentFile(new DocumentFile(
                            onDiskGuid,
                            command.FileName,
                            MediaTypes.Pdf,
                            relativeBlobPath,
                            pageCount,
                            length,
                            DataSource.Uploaded,
                            dateCreated));
                    }

                    _patentRepository.Update(document, cancellationToken);

                    await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

                    if (command.DesignatedImages.Any())
                    {
                        var relativeImgBlobPath = $"{document.ReferenceNumber}/img/".ToLower();

                        int sequence = document.DocumentImages.Select(img => img.Sequence).OrderBy(x => x).LastOrDefault();

                        var images = Conversion.ToImages(command.File).ToList();

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

                    return new UploadDocumentFileCommandResult(true);
                }
                else if(document != null)
                {
                    _logger.LogWarning($"Corrupted PDF file identified for {command.ReferenceNumber}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed {nameof(UploadDocumentFileCommand)} for Reference {command.ReferenceNumber}");
            }

            return new UploadDocumentFileCommandResult(false);
        }
    }
}