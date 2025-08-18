using System.IO.Compression;
using System.Xml.Linq;
using Patents.ArtRepoCloud.Service.Configuration;
using Patents.ArtRepoCloud.Service.DataProviders.Ifi.Interfaces;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.DataProviders.Ifi.Contracts;
using Patents.ArtRepoCloud.Service.DataProviders.IpdIfi.Contracts;
using Patents.ArtRepoCloud.Service.Enums;
using Patents.ArtRepoCloud.Service.DataProviders.IpdIfi.Interfaces;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Service.Code.Attributes;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchRequests;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses;
using Patents.ArtRepoCloud.Service.ValueObjects;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Aspose.Drawing;
using Patents.ArtRepoCloud.Domain.ValueObjects;
using Patents.ArtRepoCloud.Service.DataFetchers.Contracts;
using Patents.ArtRepoCloud.Service.Exceptions;
using Patents.ArtRepoCloud.Service.Extensions;

namespace Patents.ArtRepoCloud.Service.DataFetchers.IpdIfi
{
    [ImportSource(ImportSource.Ifi)]
    public class IpdIfiImageFetcher : IFetcher<ImageFetchRequest, ImageFetchResponse>
    {
        private readonly IIpdIfiApiProxy _ipdIfiApiProxy;
        private readonly IIfiApiProxy _ifiApiProxy;
        private readonly IFileRepository _fileRepository;
        private readonly IpdIfiApiSettings _ifiSettings;
        private readonly ILogger<IpdIfiImageFetcher> _logger;

        public IpdIfiImageFetcher(IIpdIfiApiProxy ipdIfiApiProxy, IIfiApiProxy ifiApiProxy, IFileRepository fileRepository, IpdIfiApiSettings ifiSettings, ILogger<IpdIfiImageFetcher> logger)
        {
            _ipdIfiApiProxy = ipdIfiApiProxy;
            _ifiApiProxy = ifiApiProxy;
            _fileRepository = fileRepository;
            _ifiSettings = ifiSettings;
            _logger = logger;
        }

        public async Task<ImageFetchResponse> ProcessAsync(ImageFetchRequest request, CancellationToken cancellationToken)
        {
            var referenceNumber = request.ReferenceNumber;

            _logger.LogDebug($"Started handling document retrieval request for {referenceNumber}.");

            try
            {
                var result = await _ipdIfiApiProxy.GetImageAttachmentsAsync(referenceNumber, cancellationToken).ConfigureAwait(false);

                if (result is { Status: IpdIfiRequestStatus.Success} && (result.Images?.Any() ?? false))
                {
                    _logger.LogDebug($"IpdIfi {nameof(ImageFetchRequest)} succeeded for reference #: {referenceNumber}.");

                    var attachments = await _ifiApiProxy.AttachmentListAsync(referenceNumber.Ucid, cancellationToken).ConfigureAwait(false);

                    var imagesMetadata = result.Images!
                        .Where(i => !string.IsNullOrWhiteSpace(i.FileName)).Join(attachments, left => left.FileName.ToUpper(), right => right.Filename.ToUpper(), (left, right) => new { image = left, attachment = right })
                        .Select(i => new IfiImageMetadata(i.image.FileName, i.attachment.Path, i.attachment.Media, i.image.Orientation, i.image.Sequence))
                        .ToList();

                    var filesList = await GetAttachmentsAsync(referenceNumber, imagesMetadata, cancellationToken).ConfigureAwait(false);

                    return new ImageFetchResponse(referenceNumber, filesList, HttpReasonCode.Success, ImportSource.Ifi);
                } 
                
                if (result is { Status: IpdIfiRequestStatus.NoData })
                {
                    _logger.LogDebug($"IpdIfi {nameof(ImageFetchRequest)} failed for reference #: {referenceNumber}. Reason Code {result?.Status}");

                    return new ImageFetchResponse(referenceNumber, HttpReasonCode.NoData, ImportSource.Ifi);
                }

                _logger.LogDebug($"IpdIfi {nameof(ImageFetchRequest)} failed for reference #: {referenceNumber}. Reason Code {result?.Status}");

                return Failed(HttpReasonCode.BadRequest);
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
                _logger.LogError(ex, $"{nameof(IpdIfiImageFetcher)} request failed for reference #: {referenceNumber}.");

                return Failed(HttpReasonCode.Failed);
            }

            ImageFetchResponse Failed(HttpReasonCode code) => new(request.ReferenceNumber, code, ImportSource.Ifi);
        }

        private async Task<List<ImageData>> GetAttachmentsAsync(ReferenceNumber referenceNumber, IList<IfiImageMetadata> imageMetadataList, CancellationToken cancellationToken)
        {
            var filesList = new List<ImageData>();
            var relativeIfiBlobPath = $"{referenceNumber}/ifi/images/".ToLower();

            if (imageMetadataList.Count() < _ifiSettings.AttachmentFetchAllWeightRU)
            {
                var imageDataTasks = imageMetadataList.Select(async imageMetadata =>
                    await _ifiApiProxy.AttachmentFetchAsync(imageMetadata.IfiPath, cancellationToken).ConfigureAwait(false));

                var imageDataArray = await Task.WhenAll(imageDataTasks).ConfigureAwait(false);

                var sequence = 0;
                foreach (var imageMetadata in imageMetadataList)
                {
                    var files = await ConvertToPngAndSaveAsync(relativeIfiBlobPath, imageMetadata, sequence + 1, imageDataArray[sequence], cancellationToken).ConfigureAwait(false);

                    filesList.AddRange(files);

                    sequence++;
                }
            }
            else
            {
                var archiveStream = await _ifiApiProxy.AttachmentFetchAllAsync(referenceNumber.Ucid, cancellationToken).ConfigureAwait(false);

                ZipArchive archive = new ZipArchive(archiveStream);
                var sequence = 0;

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    var imageName = entry.Name;
                    var imageMetadata = imageMetadataList.FirstOrDefault(m => m.FileName == imageName);

                    if (imageMetadata != null)
                    {
                        await using (var unzippedEntryStream = entry.Open())
                        {
                            byte[] bytes;
                            using (var ms = new MemoryStream())
                            {
                                await unzippedEntryStream.CopyToAsync(ms, cancellationToken);

                                var files = await ConvertToPngAndSaveAsync(relativeIfiBlobPath, imageMetadata, sequence + 1, ms, cancellationToken)
                                    .ConfigureAwait(false);

                                filesList.AddRange(files);
                            }

                            sequence++;
                        }
                    }
                }
            }

            return filesList;
        }

        private async Task<List<ImageData>> ConvertToPngAndSaveAsync(string relativeIfiBlobPath, IfiImageMetadata imageMetadata, int sequence, Stream imageStream, CancellationToken cancellationToken)
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

                    if (!imageMetadata.Orientation.Equals("portrait"))
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

                    var path = _fileRepository.TempPath(relativeIfiBlobPath);

                    var onDiskId = await _fileRepository.SaveAsync(outputStream, path, cancellationToken).ConfigureAwait(false);

                    filesList.Add(new ImageData(
                        onDiskId,
                        fileName,
                        relativeIfiBlobPath,
                        MediaTypes.Png,
                        length,
                        sequence));
                }
            }

            return filesList;
        }
    }
}