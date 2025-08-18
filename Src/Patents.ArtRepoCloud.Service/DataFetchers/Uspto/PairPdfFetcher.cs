using Patents.ArtRepoCloud.Domain.Interfaces;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchRequests;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses;
using Patents.ArtRepoCloud.Service.DataProviders.Uspto.Contracts;
using Patents.ArtRepoCloud.Service.DataProviders.Uspto.Interfaces;
using Patents.ArtRepoCloud.Service.Enums;
using Patents.ArtRepoCloud.Service.ValueObjects;
using Patents.ArtRepoCloud.Service.ValueObjects.PairData;
using System.Runtime.Serialization;
using Patents.ArtRepoCloud.Domain.Attributes;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Service.Exceptions;
using Patents.ArtRepoCloud.Service.Extensions;
using Patents.ArtRepoCloud.Service.Configuration;
using Patents.ArtRepoCloud.Service.DataFetchers.Uspto.Contracts;
using System.Text.RegularExpressions;
using System.IO.Compression;
using iTextSharp.text.pdf;
using Patents.ArtRepoCloud.Service.DataFetchers.Contracts;
using Patents.ArtRepoCloud.Domain.Extensions;
using Patents.ArtRepoCloud.Domain.ValueObjects;

namespace Patents.ArtRepoCloud.Service.DataFetchers.Uspto
{
    public class PairPdfFetcher : IFetcher<PairPdfFetchRequest, PairPdfFetchResponse>
    {
        private readonly IUsptoApiProxy _apiProxy;
        private readonly IFileRepository _fileRepository;
        private readonly PairSettings _pairSettings;
        private readonly ILogger<PairPdfFetcher> _logger;
        private readonly string[] _ignoredDocumentCodes = { "APP.TEXT", "NPL" };

        public PairPdfFetcher(IUsptoApiProxy apiProxy, IFileRepository fileRepository, PairSettings pairSettings, ILogger<PairPdfFetcher> logger)
        {
            _apiProxy = apiProxy;
            _fileRepository = fileRepository;
            _pairSettings = pairSettings;
            _logger = logger;
        }

        public async Task<PairPdfFetchResponse> ProcessAsync(PairPdfFetchRequest request, CancellationToken cancellationToken)
        {
            var pairNumber = request.ReferenceNumber.ToPairNumber();

            try
            {
                var fileDataListResult = new List<UsptoFileData>();

                var filesMetadata = await LoadFilesMetadata(pairNumber, cancellationToken)
                .ConfigureAwait(false);

                if (!filesMetadata.Any())
                {
                    return Failed(HttpReasonCode.NoData);
                }

                var pendingFiles = filesMetadata
                    .Where(usptoFile => request.DocumentPairFiles.All(pairFile => pairFile.ObjectId != usptoFile.ObjectId || pairFile.PairFile == null))
                    .Where(x => !_ignoredDocumentCodes.Contains(x.DocumentCode))
                    .Select(x =>
                        new FileMetadata(
                            x.PageCount,
                            x.Category,
                            x.DocumentCode,
                            x.DocumentDescription,
                            x.ObjectId,
                            x.MailRoomDate,
                            x.LogicalPackageNumber,
                            x.IFWCheckboxIndex,
                            (x.MimeTypes.FirstOrDefault(mt => mt == "pdf") ?? x.MimeTypes.First()).ToLower()));

                var initialDownloadType = UsptoFileDownloadType.PdfZip;

                var allowedDocumentDownloadTypes = Enum.GetValues(typeof(UsptoFileDownloadType)).Cast<UsptoFileDownloadType>()
                    .Where(t => (int)t >= (int)initialDownloadType).OrderBy(t => (int)t).ToList();

                var documentDownloadQueue = pendingFiles.Select((d, i) => (Index: i, Doc: d)).ToList();

                int iteration = 0;
                foreach (var downloadType in allowedDocumentDownloadTypes)
                {
                    var maxPageCountPerRequest = downloadType.Equals(UsptoFileDownloadType.PdfZip) && _pairSettings.PdfZipMaxPageCount > 0
                        ? _pairSettings.PdfZipMaxPageCount
                        : downloadType.Equals(UsptoFileDownloadType.PdfInSingleFile) && _pairSettings.PdfInSingleFileMaxPageCount > 0
                            ? _pairSettings.PdfInSingleFileMaxPageCount
                            : downloadType.GetAttribute<MaxPageCountAttribute>()?.Count ?? -1;

                    var prouppedByPageCountList = documentDownloadQueue
                        .Where(d => d.Doc.IndividualDownloadType.Equals(downloadType))
                        .GroupBy(d => documentDownloadQueue
                            .Where(item => item.Index <= d.Index)
                            .Select(item => item.Doc.PageCount).Sum() / maxPageCountPerRequest)
                        .Select(d => d.Select(gd => gd).ToList());

                    int packageNumber = 0;
                    foreach (var docsQueueItem in prouppedByPageCountList)
                    {
                        packageNumber++;

                        var fileMetadataList = docsQueueItem.Select(x => x.Doc).ToList();

                        var filesPostResult = await PostFilesRequest(
                                pairNumber,
                                request.CustomerNumber,
                                initialDownloadType,
                                fileMetadataList,
                                packageNumber,
                                cancellationToken)
                            .ConfigureAwait(false);

                        var fileStream = await DownloadFiles(
                                pairNumber,
                                filesPostResult.RequestIdentifier,
                                cancellationToken)
                            .ConfigureAwait(false);

                        var fileDataList = await SaveZipFilesAsTemp(
                                fileStream,
                                pairNumber,
                                request.CustomerNumber,
                                fileMetadataList,
                                cancellationToken)
                            .ConfigureAwait(false);

                        fileDataListResult.AddRange(fileDataList);
                    }
                }

                return new PairPdfFetchResponse(request.ReferenceNumber, fileDataListResult, HttpReasonCode.Success, ImportSource.Uspto);
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
                _logger.LogError(exp, $"Failed to retrieve the pdf for \"{request.ReferenceNumber}\".");

                return Failed(HttpReasonCode.Failed);
            }

            PairPdfFetchResponse Failed(HttpReasonCode code) => new(request.ReferenceNumber, code, ImportSource.Uspto);
        }

        async Task<IList<FileDataDto>> LoadFilesMetadata(PairNumber pairNumber, CancellationToken cancellationToken)
        {
            try
            {
                var filesMetadataResult = await _apiProxy.GetDocumentsInfo(pairNumber, cancellationToken)
                    .ConfigureAwait(false);

                if (filesMetadataResult.Success)
                {
                    return (filesMetadataResult
                        .Result?
                        .Result
                        .FirstOrDefault()?
                        .Documents.Select((d, index) => new FileDataDto(
                            d.PageTotalQuantity,
                            d.DirectionCategory,
                            d.DocumentCode,
                            d.DocumentDescription,
                            d.DocumentIdentifier,
                            d.OfficialDate,
                            d.MimeTypeBag.Select(mt => mt.ToLower()),
                            d.LogicalPackageNumber,
                            index)
                        ) ?? Enumerable.Empty<FileDataDto>()).ToList();
                }

                throw new PairActionFailureException(
                    PairActionType.ImageFileWrapper,
                    filesMetadataResult.ReasonCode,
                    filesMetadataResult.Message);
            }
            catch (Exception exp)
            {
                var isTimeOut = exp.Message.Contains("HttpClient.Timeout");

                _logger.LogError(exp, $"{nameof(PairPdfFetcher)} failed for SearchApplication: {pairNumber}. Exception Message: {exp.Message}");

                throw new PairActionFailureException(
                    PairActionType.ImageFileWrapper,
                    isTimeOut ? HttpReasonCode.RequestTimeout : HttpReasonCode.InternalError,
                    isTimeOut ? "Request Timeout." : "Application Internal Error.");
            }
        }

        async Task<PostDocumentsRequestResponseDataContract> PostFilesRequest(PairNumber pairNumber, string customerNumber, UsptoFileDownloadType downloadType, List<FileMetadata> fileMetadataList, int packageNumber, CancellationToken cancellationToken)
        {
            try
            {
                var currentDownloadType = fileMetadataList.Count > 1
                                          && !downloadType.Equals(UsptoFileDownloadType.PdfIndividualFile)
                    ? downloadType
                    : UsptoFileDownloadType.PdfIndividualFile;

                PostDocumentsRequestRequest requestPayload;

                if (currentDownloadType.Equals(UsptoFileDownloadType.PdfIndividualFile))
                {
                    requestPayload = fileMetadataList
                        .Select(am => new PostDocumentsRequestRequest(
                            fileMetadataList.First().ObjectId,
                            fileMetadataList.Select(d =>
                                new PostDocumentsRequestRequest.Document(
                                    $"{customerNumber}_{pairNumber}_{d.MailRoomDate:MM-dd-yyyy}_{d.DocumentCode}",
                                    d.ObjectId,
                                    pairNumber.Number,
                                    customerNumber,
                                    d.MailRoomDate.ToString("MM-dd-yyyy"),
                                    d.DocumentCode,
                                    d.MimeType,
                                    false,
                                    d.Category)
                            ).ToList())).First();
                }
                else
                {
                    requestPayload = fileMetadataList
                        .Select(am => new PostDocumentsRequestRequest(
                            "Multi-file-download",
                            downloadType.GetAttribute<EnumMemberAttribute>()?.Value!,
                            fileMetadataList.Select(d =>
                                new PostDocumentsRequestRequest.Document(
                                    $"{customerNumber}_{pairNumber}_{d.MailRoomDate:MM-dd-yyyy}_{d.DocumentCode}",
                                    d.ObjectId,
                                    pairNumber.Number,
                                    customerNumber,
                                    d.MailRoomDate.ToString("MM-dd-yyyy"),
                                    d.DocumentCode,
                                    d.MimeType,
                                    false,
                                    d.Category)
                            ).ToList())).First();
                }


                var fileResult = await _apiProxy.PostFilesRequestAsync(pairNumber, requestPayload, cancellationToken)
                    .ConfigureAwait(false);

                if (fileResult.Success && !string.IsNullOrEmpty(fileResult.Result?.RequestIdentifier))
                {
                    _logger.LogInformation($"Completed {downloadType.GetName()} download for SearchApplication: {pairNumber}.");

                    return fileResult.Result;
                }

                throw new PairActionFailureException(
                    PairActionType.AvailableDocuments,
                    HttpReasonCode.BadRequest,
                    HttpReasonCode.BadRequest.GetName());
            }
            catch (Exception exp)
            {
                var isTimeOut = exp.Message.Contains("HttpClient.Timeout");

                _logger.LogError(exp, $"{nameof(PairPdfFetcher)} failed for SearchApplication: {pairNumber}. Exception Message: {exp.Message}");

                throw new PairActionFailureException(
                    PairActionType.AvailableDocuments,
                    isTimeOut ? HttpReasonCode.RequestTimeout : HttpReasonCode.InternalError,
                    isTimeOut ? "Request Timeout." : "Application Internal Error.");
            }
        }

        async Task<Stream> DownloadFiles(PairNumber pairNumber, string requestIdentifier, CancellationToken cancellationToken)
        {
            try
            {
                var downloadResult = await _apiProxy.DownloadFiles(pairNumber, requestIdentifier, cancellationToken)
                    .ConfigureAwait(false);

                if (downloadResult.Success && downloadResult.Result is { Length: > 0 })
                {
                    _logger.LogInformation($"Completed {pairNumber} download for SearchApplication: {pairNumber}.");

                    return downloadResult.Result;
                }

                throw new PairActionFailureException(
                    PairActionType.AvailableDocuments,
                    HttpReasonCode.BadRequest,
                    HttpReasonCode.BadRequest.GetName());
            }
            catch (Exception exp)
            {
                var isTimeOut = exp.Message.Contains("HttpClient.Timeout");

                _logger.LogError(exp, $"{nameof(PairPdfFetcher)} failed for SearchApplication: {pairNumber}. Exception Message: {exp.Message}");

                throw new PairActionFailureException(
                    PairActionType.AvailableDocuments,
                    isTimeOut ? HttpReasonCode.RequestTimeout : HttpReasonCode.InternalError,
                    isTimeOut ? "Request Timeout." : "Application Internal Error.");
            }
        }

        async Task<IList<UsptoFileData>> SaveZipFilesAsTemp(Stream stream, PairNumber pairNumber, string customerNumber, List<FileMetadata> fileMetadataList, CancellationToken cancellationToken)
        {
            var relativeUsptoBlobPath = $"{pairNumber}/uspto/pdf/".ToLower();
            var files = new List<UsptoFileData>();

            int i = 1;

            var usptoFileNames = new HashSet<string>();

            foreach (ZipArchiveEntry entry in new ZipArchive(stream).Entries)
            {
                await using var entryStream = entry.Open();
                var cn = string.IsNullOrEmpty(customerNumber)
                    ? "NoCN"
                    : customerNumber;

                var isValid = entryStream.IsPdfValid(out int pageCount, out int length);

                if (!isValid)
                {
                    _logger.LogWarning($"Identified corrupted Pdf file for Zip archive entry {entry.FullName}");

                    continue;
                }

                using var ms = new MemoryStream();
                await entryStream.CopyToAsync(ms, cancellationToken);
                
                try
                {
                    var docs = fileMetadataList
                        .Where(d => d.PageCount == pageCount
                                     && entry.FullName.StartsWith(
                                         $"{cn}_" +
                                         $"{pairNumber}_" +
                                         $"{d.MailRoomDate:yyyy-MM-dd}_" +
                                         $"{d.DocumentCode}"))
                        .ToList();

                    FileMetadata doc;

                    if (docs.Count == 1)
                    {
                        doc = docs.First();
                    }
                    else if (docs.Count > 1)
                    {
                        var namePart = entry.FullName.Split('.')[0];
                        var nameParts = namePart.Split('_');

                        if (nameParts.Length > 4 && int.TryParse(nameParts[4], out int orderNumber))
                        {
                            doc = docs.OrderBy(x => x.LogicalPackageNumber).ElementAt(orderNumber - 1);
                        }
                        else
                        {
                            doc = docs.First(x => files.All(f => f.ObjectId != x.ObjectId));
                        }
                    }
                    else
                    {
                        continue;
                    }

                    usptoFileNames.Add(entry.FullName);

                    var fileName = BuildPAIRFilename(pairNumber.Number, doc.MailRoomDate, doc.DocumentDescription, i);

                    var path = _fileRepository.TempPath(relativeUsptoBlobPath);

                    var onDiskId = await _fileRepository.SaveAsync(ms, path, cancellationToken).ConfigureAwait(false);

                    files.Add(new UsptoFileData(
                        onDiskId,
                        fileName,
                        relativeUsptoBlobPath,
                        MediaTypes.Pdf,
                        length,
                        doc.ObjectId,
                        doc.DocumentCode,
                        doc.Category,
                        doc.DocumentDescription,
                        doc.IFWCheckboxIndex.ToString(),
                        doc.PageCount,
                        doc.MailRoomDate));
                }
                catch (Exception exp)
                {
                    Console.WriteLine(exp);
                    throw;
                }
            }

            return files;
        }

        static string BuildPAIRFilename(string applicationNumber, DateTime mailroomDate, string description, int docId)
        {
            string newSource = RemoveBadFilenameChars(applicationNumber, true);
            string newTitle = RemoveBadFilenameChars(description, true);
            return newSource
                   + "_" + mailroomDate.ToString("yyyy-MM-dd")
                   + "_" + newTitle
                   + "_" + docId
                   + ".pdf";
        }

        static string RemoveBadFilenameChars(string documentTitle, bool replaceSpaces)
        {
            string newDoc = string.Empty;
            if (replaceSpaces)
                newDoc = documentTitle.Replace(" ", "");
            else
                newDoc = documentTitle;
            string invalidChars = "[\\~#%&*{}()/:<>?|\"]+";
            Regex regEx = new Regex(invalidChars);

            return Regex.Replace(regEx.Replace(newDoc, ""), @"\s", string.Empty);
        }
    }
}