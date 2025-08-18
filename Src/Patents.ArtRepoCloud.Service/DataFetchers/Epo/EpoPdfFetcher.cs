using Patents.ArtRepoCloud.Service.ValueObjects;
using Patents.ArtRepoCloud.Service.DataProviders.Epo.Interfaces;
using Patents.ArtRepoCloud.Service.Enums;
using Patents.ArtRepoCloud.Domain.Settings;
using Patents.ArtRepoCloud.Service.Code;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Service.Code.Attributes;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchRequests;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Patents.ArtRepoCloud.Domain.ValueObjects;
using Patents.ArtRepoCloud.Service.DataFetchers.Contracts;
using Patents.ArtRepoCloud.Service.Exceptions;
using Patents.ArtRepoCloud.Service.Extensions;

namespace Patents.ArtRepoCloud.Service.DataFetchers.Epo
{
    [ImportSource(ImportSource.Epo)]
    internal class EpoPdfFetcher : IFetcher<PdfFetchRequest, PdfFetchResponse>
    {
        private readonly IEpoApiProxy _apiProxy;
        private readonly IFileRepository _fileRepository;
        private readonly IDirectorySettings _settings;
        private readonly ILogger<EpoPdfFetcher> _logger;

        public EpoPdfFetcher(IEpoApiProxy apiProxy, IFileRepository fileRepository, IDirectorySettings settings, ILogger<EpoPdfFetcher> logger)
        {
            _apiProxy = apiProxy;
            _fileRepository = fileRepository;
            _settings = settings;
            _logger = logger;
        }

        public async Task<PdfFetchResponse> ProcessAsync(PdfFetchRequest request, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Start processing {nameof(PdfFetchRequest)} for reference # {request.ReferenceNumber}.");

            try
            {
                var referenceNumber = request.ReferenceNumber;
                var relativeEpoBlobPath = $"{request.ReferenceNumber}/epo/pdf/".ToLower();

                var metadata = await _apiProxy.GetImageMetadataAsync(referenceNumber, cancellationToken).ConfigureAwait(false);

                if (metadata.NumberOfPages == 0)
                {
                    _logger.LogInformation($"No image found for {referenceNumber}.");

                    return Failed(HttpReasonCode.NoData);
                }
                
                var path = _fileRepository.TempPath(relativeEpoBlobPath);

                var allTasks = Enumerable.Range(1, metadata.NumberOfPages).Select(async pageNumber =>
                {
                    await using var pdfStream = await _apiProxy.GetAttachmentAsync(metadata.Link, pageNumber, MediaTypes.Pdf, cancellationToken).ConfigureAwait(false);
                    pdfStream.EnsureValidPdf(out int pageCount);

                    var fileName = $"{referenceNumber}_{pageNumber}.pdf";
                    var length = pdfStream.Length;

                    var onDiskId = await _fileRepository.SaveAsync(pdfStream, path, cancellationToken).ConfigureAwait(false);

                    return new FileData(
                        onDiskId,
                        fileName,
                        relativeEpoBlobPath,
                        MediaTypes.Pdf,
                        pageCount,
                        length);
                });

                var completedPdfPages = new List<FileData>();

                var images = await Task.WhenAll(allTasks).ConfigureAwait(false);
                images = images.ToArray();
                completedPdfPages.AddRange(images);

                var filePaths = completedPdfPages.OrderBy(i => i.Sequence)
                    .Select(i => $"{path}/{i.OnDiskId}")
                    .ToList();

                var readFileTasks = filePaths.Select(async filePath => await _fileRepository.GetAsync(filePath, cancellationToken));

                var pageStreamList = await Task.WhenAll(readFileTasks).ConfigureAwait(false);

                await using var outputStream = PdfMergeAdapter.Merge(pageStreamList.Where(x => x != null).ToList());

                var length = completedPdfPages.Sum(p => p.Length);
                var pageCount = completedPdfPages.Sum(p => p.PageCount);

                var onDiskId = await _fileRepository.SaveAsync(outputStream, path, cancellationToken).ConfigureAwait(false);

                var fileHandleTasks = filePaths.Select(async filePath => await _fileRepository.DeleteAsync(filePath, cancellationToken).ConfigureAwait(false));

                await Task.WhenAll(fileHandleTasks).ConfigureAwait(false);

                foreach (var filePath in filePaths)
                {
                    await _fileRepository.DeleteAsync(filePath, cancellationToken).ConfigureAwait(false);
                }

                var pdfFileData = new FileData(onDiskId, $"{referenceNumber}.pdf", request.ReferenceNumber.ToString(), MediaTypes.Pdf, pageCount, length);

                _logger.LogDebug($"Completed {nameof(PdfFetchRequest)} for reference # {request.ReferenceNumber}.");

                return new PdfFetchResponse(referenceNumber, new List<FileData> { pdfFileData }, HttpReasonCode.Success, ImportSource.Epo);
            }
            catch (ImportProcessFailureReasonException exp)
            {
                _logger.LogInformation($"{exp.ImportSource.GetName()} pdf import failed for \"{request.ReferenceNumber}\". Reason: ({exp.ReasonCode}){exp.ReasonCode.GetName()}.");

                return Failed(exp.ReasonCode);
            }
            catch (CorruptedFileException exp)
            {
                _logger.LogError($"Identified corrupted Pdf file for {request.ReferenceNumber}. {exp.Message}");

                return Failed(HttpReasonCode.BadData);
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, $"{nameof(EpoPdfFetcher)} request failed for reference #: {request.ReferenceNumber}.");

                return Failed(HttpReasonCode.Failed);
            }

            PdfFetchResponse Failed(HttpReasonCode code) => new(request.ReferenceNumber, code, ImportSource.Epo);
        }
    }
}