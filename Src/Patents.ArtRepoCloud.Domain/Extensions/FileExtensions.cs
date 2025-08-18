using iTextSharp.text.pdf;

namespace Patents.ArtRepoCloud.Domain.Extensions
{
    public static class FileExtensions
    {
        public static bool IsPdfValid(this Stream? stream, out int pageCount, out int length)
        {
            bool isFileOk = false;
            pageCount = 0;
            length = 0;

            if (stream != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    try
                    {
                        stream.CopyTo(ms);

                        ms.Position = 0;

                        PdfReader reader = new PdfReader(ms);

                        length = (int)reader.FileLength;

                        pageCount = reader.NumberOfPages;
                        isFileOk = pageCount > 0;

                        reader.Close();
                    }
                    catch
                    {
                        isFileOk = false;
                    }
                }

                stream.Position = 0;
            }

            return isFileOk;
        }
    }
}