using System.IO.Compression;
using Patents.ArtRepoCloud.Service.Exceptions;
using iTextSharp.text.pdf;

namespace Patents.ArtRepoCloud.Service.Extensions
{
    public static class FileExtensions
    {
        public static void EnsureValidPdf(this Stream stream, out int pageCount)
        {
            pageCount = 0;

            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    stream.CopyTo(ms);

                    ms.Position = 0;

                    PdfReader reader = new PdfReader(ms);

                    pageCount = reader.NumberOfPages;

                    reader.Close();

                    if (pageCount <= 0)
                        throw new CorruptedFileException($"Identified invalid pdf file: NumberOfPages = {pageCount}!");
                }
                catch
                {
                    throw new CorruptedFileException("Identified invalid pdf file: can't read from the file!");
                }
            }

            stream.Position = 0;
        }
    }
}