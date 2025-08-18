using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.IntegrationEvents;
using Patents.ArtRepoCloud.Service.Application.Commands.EnqueueImages;
using Patents.ArtRepoCloud.Service.Application.Commands.SaveArtRepoDocumentImages;
using Patents.ArtRepoCloud.Service.DataFetchers;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchRequests;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses;
using Patents.ArtRepoCloud.Service.Enums;
using Patents.ArtRepoCloud.Service.Extensions;
using Vikcher.Framework.Common;
using MediatR;

namespace Patents.ArtRepoCloud.Service.Application.IntegrationEventHandlers
{
    internal class ArtRepoImageIntegrationEventHandler : INotificationHandler<ArtRepoImageIntegrationEvent>
    {
        private readonly IMediator _mediator;
        private readonly IFetcher<ImageFetchRequest, ImageFetchResponse>[] _fetchers;
        private readonly ILogger<ArtRepoImageIntegrationEventHandler> _logger;

        public ArtRepoImageIntegrationEventHandler(IMediator mediator, IFetcher<ImageFetchRequest, ImageFetchResponse>[] fetchers, ILogger<ArtRepoImageIntegrationEventHandler> logger)
        {
            _mediator = mediator;
            _fetchers = fetchers;
            _logger = logger;
        }

        public async Task Handle(ArtRepoImageIntegrationEvent evt, CancellationToken cancellationToken)
        
        {
            _logger.LogInformation($"Handling integration event/queue {nameof(ArtRepoImageIntegrationEvent)} for reference {evt.ReferenceNumber}.");

            foreach (var fetcher in _fetchers.StartAt(evt.ImportSource, evt.UseSourceStrictly))
            {
                if (fetcher.IsTypeOf(ImportSource.Uspto) && !evt.ReferenceNumber.IsUs())
                {
                    continue;
                }

                var response = await fetcher.ProcessAsync(new ImageFetchRequest(evt.ReferenceNumber), cancellationToken).ConfigureAwait(false);

                switch (response.HttpReasonCode)
                {
                    case HttpReasonCode.None:
                        throw new InvalidOperationException("System error. The image fetch cannot be started.");
                    case HttpReasonCode.Failed:
                        throw new Exception("This image attempt has failed.");
                    case HttpReasonCode.NoData:
                        if (Array.IndexOf(_fetchers, fetcher) < _fetchers.Length - 1)
                        {
                            _logger.LogInformation($"No image found for \"{evt.ReferenceNumber}\". Proceeding to next provider.");

                            continue;
                        }

                        _logger.LogInformation($"No image found for \"{evt.ReferenceNumber}\". The process has ended.");

                        break;
                    case HttpReasonCode.Success:
                        var saveImagesCmd = new SaveArtRepoDocumentImagesCommand(evt.ReferenceNumber, response.Files, response.Source);

                        await _mediator.Send(saveImagesCmd, cancellationToken).ConfigureAwait(false);

                        return;
                    case HttpReasonCode.ToBeContinued:
                        var enqueueImagesCmd = new EnqueueImagesCommand(
                            response.ReferenceNumber,
                            evt.Priority,
                            response.Source,
                            false,
                            (byte)(evt.RetryCount + 1));

                        await _mediator.Send(enqueueImagesCmd, cancellationToken).ConfigureAwait(false);

                        break;
                    default:
                        _logger.LogError($"Unknown fetch status result. {nameof(ArtRepoImageIntegrationEvent)}:{evt.ToJson()}.");
                        break;
                }
            }
        }
    }
}