using Aspose.Drawing;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Service.Code.Attributes;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchRequests;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Patents.ArtRepoCloud.Service.DataProviders.Epo.Interfaces;
using Patents.ArtRepoCloud.Domain.ValueObjects;
using Patents.ArtRepoCloud.Service.Enums;
using Patents.ArtRepoCloud.Service.ValueObjects;
using Patents.ArtRepoCloud.Service.DataProviders.Ifi.Contracts;
using Patents.ArtRepoCloud.Service.DataFetchers.Contracts;
using Patents.ArtRepoCloud.Service.Exceptions;
using Patents.ArtRepoCloud.Service.Extensions;
using System.IO;
using Patents.ArtRepoCloud.Domain.Extensions;

namespace Patents.ArtRepoCloud.Service.DataFetchers.Epo
{
    [ImportSource(ImportSource.Epo)]
    internal class EpoImageFetcher : IFetcher<ImageFetchRequest, ImageFetchResponse>
    {
        private readonly IEpoApiProxy _apiProxy;
        private readonly IFileRepository _fileRepository;
        private readonly ILogger<EpoImageFetcher> _logger;

        public  EpoImageFetcher(IEpoApiProxy apiProxy, IFileRepository fileRepository, ILogger<EpoImageFetcher> logger)
        {
            _apiProxy = apiProxy;
            _fileRepository = fileRepository;
            _logger = logger;
        }

        public async Task<ImageFetchResponse> ProcessAsync(ImageFetchRequest request, CancellationToken cancellationToken)
        {
            var referenceNumber = request.ReferenceNumber;
            var relativeEpoBlobPath = $"{request.ReferenceNumber}/epo/images/".ToLower();

            _logger.LogDebug($"Start processing {nameof(PdfFetchRequest)} for reference # {referenceNumber}.");

            try
            {
                var metadata = await _apiProxy.GetImageMetadataAsync(referenceNumber, cancellationToken).ConfigureAwait(false);

                if (metadata.NumberOfPages == 0)
                {
                    _logger.LogInformation($"No image found for {referenceNumber}.");

                    return new ImageFetchResponse(referenceNumber, HttpReasonCode.NoData, ImportSource.Epo);
                }
                
                var filesList = new List<ImageData>();

                var allTasks = Enumerable.Range(1, metadata.NumberOfPages).Select(async (pageNumber, index) =>
                {
                    await using var stream = await _apiProxy
                        .GetAttachmentAsync(metadata.Link, pageNumber, MediaTypes.Tiff, cancellationToken)
                        .ConfigureAwait(false);

                    var fileName = $"{referenceNumber}_{pageNumber}.pdf";

                    var imageMetadata = new IfiImageMetadata(fileName, metadata.Link, MediaTypes.Tiff, string.Empty, 1);

                    var files = await ConvertToPngAndSaveAsync(relativeEpoBlobPath, imageMetadata, index, stream, cancellationToken).ConfigureAwait(false);

                    filesList.AddRange(files);
                });

                await Task.WhenAll(allTasks).ConfigureAwait(false);

                _logger.LogDebug($"Completed {nameof(EpoImageFetcher)} request for reference # {request.ReferenceNumber}.");

                return new ImageFetchResponse(referenceNumber, filesList, HttpReasonCode.Success, ImportSource.Epo);
            }
            catch (ImportProcessFailureReasonException exp)
            {
                _logger.LogInformation($"{exp.ImportSource.GetName()} images import failed for \"{request.ReferenceNumber}\". Reason: ({exp.ReasonCode}){exp.ReasonCode.GetName()}.");

                return Failed(exp.ReasonCode);
            }
            catch (CorruptedFileException exp)
            {
                _logger.LogError($"Identified corrupted file for {referenceNumber}. {exp.Message}");

                return Failed(HttpReasonCode.BadData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(EpoImageFetcher)} request failed for reference #: {referenceNumber}.");

                return Failed(HttpReasonCode.Failed);
            }

            ImageFetchResponse Failed(HttpReasonCode code) => new(request.ReferenceNumber, code, ImportSource.Epo);
        }

        private async Task<List<ImageData>> ConvertToPngAndSaveAsync(string relativeEpoBlobPath, IfiImageMetadata imageMetadata, int sequence, Stream imageStream, CancellationToken cancellationToken)
        {
            var filesList = new List<ImageData>();

            var image = Image.FromStream(imageStream);
            var bitmap = new Aspose.Drawing.Bitmap(image);
            var pages = bitmap.GetFrameCount(Aspose.Drawing.Imaging.FrameDimension.Page);

            for (var page = 0; page < pages; page++)
            {
                image.SelectActiveFrame(Aspose.Drawing.Imaging.FrameDimension.Page, page);

                using (var outputStream = new MemoryStream())
                {
                    image.Save(outputStream, Aspose.Drawing.Imaging.ImageFormat.Png);

                    if (!imageMetadata.Orientation?.Equals("portrait") ?? false)
                    {
                        var png = Image.FromStream(outputStream);
                        png.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        outputStream.Position = 0;
                        png.Save(outputStream, Aspose.Drawing.Imaging.ImageFormat.Png);
                    }

                    outputStream.Seek(0, SeekOrigin.Begin);

                    var filenameBase = Path.GetFileNameWithoutExtension(imageMetadata.FileName);
                    var fileName = filenameBase + (page > 0 ? page.ToString() : "") + ".png";
                    var length = outputStream.Length;
                    var path = _fileRepository.TempPath(relativeEpoBlobPath);

                    var onDiskId = await _fileRepository.SaveAsync(outputStream, path, cancellationToken).ConfigureAwait(false);

                    filesList.Add(new ImageData(
                        onDiskId,
                        fileName,
                        relativeEpoBlobPath,
                        MediaTypes.Png,
                        length,
                        sequence));
                }
            }

            return filesList;
        }
    }
}