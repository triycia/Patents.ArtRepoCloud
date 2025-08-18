using Patents.ArtRepoCloud.Service.DataProviders.Ifi.Interfaces;
using Patents.ArtRepoCloud.Service.ValueObjects;
using Patents.ArtRepoCloud.Service.Extensions;
using Patents.ArtRepoCloud.Service.Enums;
using Patents.ArtRepoCloud.Service.Exceptions;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Service.Code.Attributes;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchRequests;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Patents.ArtRepoCloud.Domain.ValueObjects;
using Patents.ArtRepoCloud.Service.DataFetchers.Contracts;

namespace Patents.ArtRepoCloud.Service.DataFetchers.Ifi
{
    [ImportSource(ImportSource.Ifi)]
    public class IfiPdfFetcher : IFetcher<PdfFetchRequest, PdfFetchResponse>
    {
        private readonly IIfiApiProxy _apiProxy;
        private readonly IFileRepository _fileRepository;
        private readonly ILogger<IfiPdfFetcher> _logger;

        public IfiPdfFetcher(IIfiApiProxy apiProxy, IFileRepository fileRepository, ILogger<IfiPdfFetcher> logger)
        {
            _apiProxy = apiProxy;
            _fileRepository = fileRepository;
            _logger = logger;
        }

        public async Task<PdfFetchResponse> ProcessAsync(PdfFetchRequest request, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Start processing {nameof(PdfFetchRequest)} for reference # {request.ReferenceNumber}.");

            var relativeIfiBlobPath = $"{request.ReferenceNumber}/ifi/pdf/".ToLower();
            var referenceNumber = request.ReferenceNumber;

            try
            {
                var attachments = await _apiProxy.AttachmentListAsync(request.ReferenceNumber.Ucid, cancellationToken).ConfigureAwait(false);

                var pdfAttachment = attachments.FirstOrDefault(a => a.Media.Equals(MediaTypes.Pdf, StringComparison.CurrentCultureIgnoreCase));

                if (pdfAttachment == null)
                {
                    return new PdfFetchResponse(referenceNumber, HttpReasonCode.NoData, ImportSource.Ifi);
                }

                await using var pdfStream = await _apiProxy.AttachmentFetchAsync(pdfAttachment.Path, cancellationToken).ConfigureAwait(false);
                pdfStream.EnsureValidPdf(out int pageCount);

                var length = pdfStream.Length;
                var path = _fileRepository.TempPath(relativeIfiBlobPath);

                var onDiskId = await _fileRepository.SaveAsync(pdfStream, path, cancellationToken).ConfigureAwait(false);

                var fileData = new FileData(
                    onDiskId,
                    pdfAttachment.Filename,
                    relativeIfiBlobPath,
                    MediaTypes.Pdf,
                    pageCount,
                    length);

                return new PdfFetchResponse(referenceNumber, new List<FileData> { fileData }, HttpReasonCode.Success, ImportSource.Ifi);
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
            catch (Exception exp)
            {
                _logger.LogError(exp, $"{nameof(IfiPdfFetcher)} request failed for reference #: {referenceNumber}.");

                return Failed(HttpReasonCode.Failed);
            }

            PdfFetchResponse Failed(HttpReasonCode code) => new (referenceNumber, code, ImportSource.Ifi);
        }
    }
}