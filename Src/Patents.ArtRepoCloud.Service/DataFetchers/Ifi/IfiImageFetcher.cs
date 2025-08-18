using Patents.ArtRepoCloud.Service.DataProviders.Ifi.Interfaces;
using System.Xml.Linq;
using Patents.ArtRepoCloud.Service.DataProviders.Ifi.Contracts;
using Patents.ArtRepoCloud.Service.Configuration;
using System.IO.Compression;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Service.Enums;
using Image = Aspose.Drawing.Image;
using RotateFlipType = Aspose.Drawing.RotateFlipType;
using Patents.ArtRepoCloud.Service.Code.Attributes;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchRequests;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Patents.ArtRepoCloud.Domain.ValueObjects;
using Patents.ArtRepoCloud.Service.ValueObjects;
using Patents.ArtRepoCloud.Service.DataFetchers.Contracts;
using Patents.ArtRepoCloud.Service.Exceptions;
using Patents.ArtRepoCloud.Service.Extensions;

namespace Patents.ArtRepoCloud.Service.DataFetchers.Ifi
{
    [ImportSource(ImportSource.Ifi)]
    public class IfiImageFetcher : IFetcher<ImageFetchRequest, ImageFetchResponse>
    {
        private readonly IIfiApiProxy _apiProxy;
        private readonly IFileRepository _fileRepository;
        private readonly IficlaimsSettings _ificlaimsSettings;
        private readonly ILogger<IfiImageFetcher> _logger;

        public IfiImageFetcher(IIfiApiProxy apiProxy, IFileRepository fileRepository, IficlaimsSettings ificlaimsSettings, ILogger<IfiImageFetcher> logger)
        {
            _apiProxy = apiProxy;
            _fileRepository = fileRepository;
            _ificlaimsSettings = ificlaimsSettings;
            _logger = logger;
        }

        public async Task<ImageFetchResponse> ProcessAsync(ImageFetchRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var referenceNumber = request.ReferenceNumber;
                var relativeIfiBlobPath = $"{request.ReferenceNumber}/ifi/images/".ToLower();

                var xmlData = await _apiProxy.GetDocumentAsync(referenceNumber.Ucid, cancellationToken).ConfigureAwait(false);

                var ifiDoc = await IfiXDocument.LoadAsync(xmlData, LoadOptions.None, referenceNumber.NumberType, cancellationToken).ConfigureAwait(false);

                if (!(ifiDoc.Images.Any()))
                {
                    _logger.LogInformation($"No image metadata found for ucid \"{referenceNumber.Ucid}\".");

                    return Failed(HttpReasonCode.NoData);
                }

                var attachments = await _apiProxy.AttachmentListAsync(referenceNumber.Ucid, cancellationToken).ConfigureAwait(false);

                if (!attachments.Any())
                {
                    _logger.LogInformation($"No image attachment found for ucid \"{referenceNumber.Ucid}\".");

                    return Failed(HttpReasonCode.NoData);
                }

                var filesList = new List<ImageData>();

                var imagesMetadata = ifiDoc.Images
                    .Where(i => !string.IsNullOrWhiteSpace(i.File))
                    .Join(attachments, left => left.File.ToUpper(), right => right.Filename.ToUpper(),
                        (left, right) => new { image = left, attachment = right })
                    .Select((i, index) => new IfiImageMetadata(
                        i.image.File, 
                        i.attachment.Path, 
                        i.attachment.Media, 
                        i.image.Orientation, 
                        index + 1))
                    .ToList();

                if (imagesMetadata.Count() < _ificlaimsSettings.IfiAttachmentFetchAllWeightRU)
                {
                    foreach (var imageMetadata in imagesMetadata)
                    {
                        var stream = await _apiProxy.AttachmentFetchAsync(imageMetadata.IfiPath, cancellationToken).ConfigureAwait(false);

                        var length = stream.Length;
                        var path = _fileRepository.TempPath(relativeIfiBlobPath);

                        var onDiskId = await _fileRepository.SaveAsync(stream, path, cancellationToken).ConfigureAwait(false);

                        filesList.Add(new ImageData(
                            onDiskId,
                            imageMetadata.FileName,
                            relativeIfiBlobPath,
                            MediaTypes.Png,
                            length,
                            imageMetadata.Sequence));
                    }
                }
                else
                {
                    await using var archiveStream = await _apiProxy
                        .AttachmentFetchAllAsync(referenceNumber.Ucid, cancellationToken).ConfigureAwait(false);

                    ZipArchive archive = new ZipArchive(archiveStream);
                    
                    var sequence = 0;

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        var imageName = entry.Name;
                        var imageMetadata = imagesMetadata.FirstOrDefault(m => m.FileName == imageName);

                        if (imageMetadata != null)
                        {
                            await using var unzippedEntryStream = entry.Open();

                            using (var ms = new MemoryStream())
                            {
                                await unzippedEntryStream.CopyToAsync(ms, cancellationToken);

                                var files = await ConvertToPngAndSaveAsync(
                                        relativeIfiBlobPath,
                                        imageMetadata,
                                        sequence + 1,
                                        ms,
                                        cancellationToken)
                                    .ConfigureAwait(false);

                                filesList.AddRange(files);
                            }

                            sequence++;
                        }
                    }
                }

                return new ImageFetchResponse(referenceNumber, filesList, HttpReasonCode.Success, ImportSource.Ifi);
            }
            catch (ImportProcessFailureReasonException exp)
            {
                _logger.LogInformation($"{exp.ImportSource.GetName()} images import failed for \"{request.ReferenceNumber}\". Reason: ({exp.ReasonCode}){exp.ReasonCode.GetName()}.");

                return Failed(exp.ReasonCode);
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, $"Failed to retrieve the images for \"{request.ReferenceNumber}\".");

                return Failed(HttpReasonCode.Failed);
            }

            ImageFetchResponse Failed(HttpReasonCode code) => new(request.ReferenceNumber, code, ImportSource.Ifi);
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

                using var outputStream = new MemoryStream();
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

            return filesList;
        }
    }
}