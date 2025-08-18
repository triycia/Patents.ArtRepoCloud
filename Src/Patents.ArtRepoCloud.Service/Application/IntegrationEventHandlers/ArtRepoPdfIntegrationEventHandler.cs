using Patents.ArtRepoCloud.Domain.IntegrationEvents;
using MediatR;
using Patents.ArtRepoCloud.Service.DataFetchers;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchRequests;
using Patents.ArtRepoCloud.Service.Enums;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Service.Extensions;
using Patents.ArtRepoCloud.Service.Application.Commands.EnqueuePdf;
using Patents.ArtRepoCloud.Service.Application.Commands.SaveArtRepoDocumentPdfs;
using Vikcher.Framework.Common;

namespace Patents.ArtRepoCloud.Service.Application.IntegrationEventHandlers
{
    internal class ArtRepoPdfIntegrationEventHandler : INotificationHandler<ArtRepoPdfIntegrationEvent>
    {
        private readonly IMediator _mediator;
        private readonly IFetcher<PdfFetchRequest, PdfFetchResponse>[] _fetchers;
        private readonly ILogger<ArtRepoPdfIntegrationEventHandler> _logger;

        public ArtRepoPdfIntegrationEventHandler(IMediator mediator, IFetcher<PdfFetchRequest, PdfFetchResponse>[] fetchers, ILogger<ArtRepoPdfIntegrationEventHandler> logger)
        {
            _mediator = mediator;
            _fetchers = fetchers;
            _logger = logger;
        }

        public async Task Handle(ArtRepoPdfIntegrationEvent evt, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Handling integration event {nameof(ArtRepoPdfIntegrationEvent)} for reference {evt.ReferenceNumber}.");

            foreach (var fetcher in _fetchers.StartAt(evt.ImportSource, evt.UseSourceStrictly))
            {
                if (fetcher.IsTypeOf(ImportSource.Uspto) && !evt.ReferenceNumber.IsUs())
                {
                    continue;
                }

                var response = await fetcher.ProcessAsync(new PdfFetchRequest(evt.ReferenceNumber), cancellationToken).ConfigureAwait(false);

                switch (response.HttpReasonCode)
                {
                    case HttpReasonCode.None:
                        throw new InvalidOperationException("System error. The pdf fetch cannot be started.");
                    case HttpReasonCode.Failed:
                        throw new Exception("This pdf attempt has failed. Throwing an exception to put the Queue to Dead-letter.");
                    case HttpReasonCode.NoData:
                        if (Array.IndexOf(_fetchers, fetcher) < _fetchers.Length - 1)
                        {
                            _logger.LogInformation($"No pdf found for \"{evt.ReferenceNumber}\". Proceeding to next provider.");

                            continue;
                        }

                        _logger.LogInformation($"No pdf found for \"{evt.ReferenceNumber}\". The process has ended.");

                        break;
                    case HttpReasonCode.Success:
                        var saveImagesCmd = new SaveArtRepoDocumentPdfsCommand(evt.ReferenceNumber, response.PdfFiles, response.Source);

                        await _mediator.Send(saveImagesCmd, cancellationToken).ConfigureAwait(false);

                        return;
                    case HttpReasonCode.ToBeContinued:
                        var enqueuePdfCmd = new EnqueuePdfCommand(
                            response.ReferenceNumber, 
                            evt.Priority, 
                            ImportSource.All, 
                            evt.UseSourceStrictly,
                            (byte)(evt.RetryCount + 1));

                        await _mediator.Send(enqueuePdfCmd, cancellationToken).ConfigureAwait(false);

                        break;
                    default:
                        _logger.LogError($"Unknown fetch status result. {nameof(ArtRepoPdfIntegrationEvent)}:{evt.ToJson()}.");
                        break;
                }
            }
        }
    }
}