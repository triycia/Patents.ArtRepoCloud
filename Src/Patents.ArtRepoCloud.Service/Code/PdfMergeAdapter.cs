using Vikcher.Framework.Common;
using iTextSharp.text.pdf;
using NLog;
using System.Diagnostics.CodeAnalysis;

namespace Patents.ArtRepoCloud.Service.Code
{
    public static class PdfMergeAdapter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static void Merge(string targetFilePath, IEnumerable<(string referenceNumber, string title, byte[] fileBytes)> sourceFilesInfo)
        {
            Guard.AssertNotNullOrEmpty(sourceFilesInfo, nameof(sourceFilesInfo));
            var pdfBookmarks = new List<Dictionary<string, object>>();
            var outStream = new FileStream(targetFilePath, FileMode.Create);
            var document = new iTextSharp.text.Document();
            var pdfCopy = new PdfCopy(document, outStream);

            document.Open();

            var pageOffset = 0;

            foreach (var fileInfo in sourceFilesInfo.Where(f => f.fileBytes != null))
            {
                try
                {
                    using (var pdfReader = new PdfReader(fileInfo.fileBytes))
                    {
                        CreatePdfBookmark(fileInfo.referenceNumber, fileInfo.title);
                        CreatePdfPages();

                        void CreatePdfPages()
                        {
                            pageOffset += pdfReader.NumberOfPages;

                            for (var i = 1; i <= pdfReader.NumberOfPages; i++)
                            {
                                var page = pdfCopy.GetImportedPage(pdfReader, i);
                                pdfCopy.AddPage(page);
                            }
                        }

                        void CreatePdfBookmark(string referenceNumber, string title)
                        {
                            var pdfBookmark = new Dictionary<string, object>
                            {
                                {"Title", referenceNumber + " " + title},
                                {"Action", "GoTo"},
                                {"Page", (pageOffset + 1) + " XYZ"}
                            };

                            var tempBookmarks = SimpleBookmark.GetBookmark(pdfReader);
                            SimpleBookmark.ShiftPageNumbers(tempBookmarks, pageOffset, null);
                            pdfBookmark.Add("Kids", tempBookmarks);
                            pdfBookmarks.Add(pdfBookmark);
                        }
                    }
                }
                catch (iTextSharp.text.exceptions.InvalidPdfException)
                {
                    Logger.Warn($"ProcessItem: Error adding file to pdf file: {fileInfo.referenceNumber}'s associated file is an invalid Pdf. Skipping.");
                }
            }

            pdfCopy.Outlines = pdfBookmarks.ToList();
            document.Close();
        }

        public static Stream Merge(List<Stream?> fileStreams)
        {
            Guard.AssertNotNullOrEmpty(fileStreams, nameof(fileStreams));
            var outStream = new MemoryStream();

            using (var document = new iTextSharp.text.Document())
            {
                using (var pdfCopy = new PdfCopy(document, outStream))
                {
                    pdfCopy.CloseStream = false;

                    document.Open();

                    foreach (var stream in fileStreams.Where(f => f != null))
                    {
                        try
                        {
                            using (var pdfReader = new PdfReader(stream))
                            {
                                CreatePdfPages();

                                void CreatePdfPages()
                                {
                                    for (var i = 1; i <= pdfReader.NumberOfPages; i++)
                                    {
                                        var page = pdfCopy.GetImportedPage(pdfReader, i);
                                        pdfCopy.AddPage(page);
                                    }
                                }
                            }
                        }
                        catch (iTextSharp.text.exceptions.InvalidPdfException ex)
                        {
                            Logger.Warn($"ProcessItem: Error merging pdf file because it is an invalid Pdf. Skipping.", ex);
                        }
                    }
                }
            }

            outStream.Seek(0, SeekOrigin.Begin);

            return outStream;
        }


        public static Stream Merge(byte[][] fileBytesArray)
        {
            Guard.AssertNotNullOrEmpty(fileBytesArray, nameof(fileBytesArray));
            var outStream = new MemoryStream();

            using (var document = new iTextSharp.text.Document())
            {
                using (var pdfCopy = new PdfCopy(document, outStream))
                {
                    pdfCopy.CloseStream = false;

                    document.Open();

                    foreach (var bytes in fileBytesArray.Where(f => f != null))
                    {
                        try
                        {
                            using (var pdfReader = new PdfReader(bytes))
                            {
                                CreatePdfPages();

                                void CreatePdfPages()
                                {
                                    for (var i = 1; i <= pdfReader.NumberOfPages; i++)
                                    {
                                        var page = pdfCopy.GetImportedPage(pdfReader, i);
                                        pdfCopy.AddPage(page);
                                    }
                                }
                            }
                        }
                        catch (iTextSharp.text.exceptions.InvalidPdfException ex)
                        {
                            Logger.Warn($"ProcessItem: Error merging pdf file because it is an invalid Pdf. Skipping.", ex);
                        }
                    }
                }
            }

            outStream.Seek(0, SeekOrigin.Begin);

            return outStream;
        }
    }
}