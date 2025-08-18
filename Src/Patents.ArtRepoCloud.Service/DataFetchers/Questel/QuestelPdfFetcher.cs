using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Patents.ArtRepoCloud.Domain.ValueObjects;
using Patents.ArtRepoCloud.Service.Code.Attributes;
using Patents.ArtRepoCloud.Service.DataFetchers.Contracts;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchRequests;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses;
using Patents.ArtRepoCloud.Service.DataProviders.Questel.Interfaces;
using Patents.ArtRepoCloud.Service.Enums;
using Patents.ArtRepoCloud.Service.Exceptions;
using Patents.ArtRepoCloud.Service.Extensions;
using Patents.ArtRepoCloud.Service.ValueObjects;

namespace Patents.ArtRepoCloud.Service.DataFetchers.Questel
{
    [ImportSource(ImportSource.Questel)]
    public class QuestelPdfFetcher : IFetcher<PdfFetchRequest, PdfFetchResponse>
    {
        private readonly IQuestelApiProxy _apiProxy;
        private readonly IFileRepository _fileRepository;
        private readonly ILogger<QuestelPdfFetcher> _logger;

        public QuestelPdfFetcher(IQuestelApiProxy apiProxy, IFileRepository fileRepository, ILogger<QuestelPdfFetcher> logger)
        {
            _apiProxy = apiProxy;
            _fileRepository = fileRepository;
            _logger = logger;
        }

        public async Task<PdfFetchResponse> ProcessAsync(PdfFetchRequest request, CancellationToken cancellationToken)
        {
            var referenceNumber = request.ReferenceNumber;
            var relativeQuestelBlobPath = $"{referenceNumber}/questel/pdf/".ToLower();

            _logger.LogDebug($"Start processing {nameof(QuestelPdfFetcher)} for reference # {referenceNumber}.");

            try
            {
                await using var pdfStream = await _apiProxy.DownloadPdfAsync(referenceNumber, cancellationToken).ConfigureAwait(false);
                pdfStream.EnsureValidPdf(out int pageCount);

                var fileName = $"{referenceNumber}.pdf";
                var length = pdfStream.Length;
                var path = _fileRepository.TempPath(relativeQuestelBlobPath);

                var onDiskId = await _fileRepository.SaveAsync(pdfStream, path, cancellationToken).ConfigureAwait(false);

                var fileData = new FileData(
                    onDiskId,
                    fileName,
                    relativeQuestelBlobPath,
                    MediaTypes.Pdf,
                    pageCount,
                    length);

                return new PdfFetchResponse(referenceNumber, new List<FileData> { fileData }, HttpReasonCode.Success, ImportSource.Questel);
            }
            catch (ImportProcessFailureReasonException exp)
            {
                _logger.LogInformation($"{exp.ImportSource.GetName()} pdf import failed for \"{request.ReferenceNumber}\". Reason: ({exp.ReasonCode}){exp.ReasonCode.GetName()}.");

                return Failed(exp.ReasonCode);
            }
            catch (CorruptedFileException exp)
            {
                _logger.LogError($"Identified corrupted Pdf file for {referenceNumber}. {exp.Message}");

                return Failed(HttpReasonCode.BadData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(QuestelPdfFetcher)} request Failed for reference #: {referenceNumber}.");

                return Failed(HttpReasonCode.Failed);
            }

            PdfFetchResponse Failed(HttpReasonCode code) => new(referenceNumber, code, ImportSource.Questel);
        }
    }
}