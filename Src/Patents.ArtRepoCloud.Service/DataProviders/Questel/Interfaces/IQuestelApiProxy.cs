using Patents.ArtRepoCloud.Domain.ReferenceNumbers;

namespace Patents.ArtRepoCloud.Service.DataProviders.Questel.Interfaces
{
    public interface IQuestelApiProxy
    {
        Task<Stream> DownloadPdfAsync(ReferenceNumber referenceNumber, CancellationToken cancellationToken);
    }
}