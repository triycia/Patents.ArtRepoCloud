using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.IntegrationEvents;
using Patents.ArtRepoCloud.Service.Application.Commands.EnqueuePdf;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchRequests;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses;
using Patents.ArtRepoCloud.Service.DataFetchers;
using MediatR;
using Patents.ArtRepoCloud.Service.Application.Commands.RequeueDocument;
using Patents.ArtRepoCloud.Service.Application.Commands.SavePairDocument;
using Patents.ArtRepoCloud.Service.Enums;
using Vikcher.Framework.Common;

namespace Patents.ArtRepoCloud.Service.Application.IntegrationEventHandlers
{
    internal class PAIRDocumentIntegrationEventHandler : INotificationHandler<PAIRDocumentIntegrationEvent>
    {
        private readonly IMediator _mediator;
        private readonly IFetcher<PairDocumentFetchRequest, PairDocumentFetchResponse> _fetcher;
        private readonly ILogger<ArtRepoDocumentIntegrationEventHandler> _logger;

        public PAIRDocumentIntegrationEventHandler(IMediator mediator, IFetcher<PairDocumentFetchRequest, PairDocumentFetchResponse> fetcher, ILogger<ArtRepoDocumentIntegrationEventHandler> logger)
        {
            _mediator = mediator;
            _fetcher = fetcher;
            _logger = logger;
        }

        public async Task Handle(PAIRDocumentIntegrationEvent evt, CancellationToken cancellationToken)
        {
            var response = await _fetcher.ProcessAsync(new PairDocumentFetchRequest(evt.ReferenceNumber), cancellationToken).ConfigureAwait(false);

            switch (response.HttpReasonCode)
            {
                case HttpReasonCode.None:
                    throw new InvalidOperationException("System error. The USPTO data fetch cannot be started.");
                case HttpReasonCode.Failed:
                    var msg =
                        $"This pdf download has failed at {evt.RetryCount} USPTO data source. Throwing an exception to put the Queue to Dead-letter.";

                    _logger.LogWarning(msg);

                    throw new Exception(msg);
                case HttpReasonCode.NoData:
                    _logger.LogInformation($"No document data found on USPTO for \"{evt.ReferenceNumber}\". The process has ended.");

                    break;
                case HttpReasonCode.Success:
                    var saveDocCmd = new SavePairDocumentCommand(
                        response.ReferenceNumber, 
                        response.ApplicationData, 
                        response.Continuity, 
                        response.Transactions, 
                        response.AttorneyAgents);
                    
                    await _mediator.Send(saveDocCmd, cancellationToken).ConfigureAwait(false);

                    var enqueuePdfCmd = new EnqueuePdfCommand(
                        response.ReferenceNumber, 
                        evt.Priority, 
                        ImportSource.Uspto,
                        0);

                    await _mediator.Send(enqueuePdfCmd, cancellationToken).ConfigureAwait(false);
                    
                    break;
                case HttpReasonCode.ToBeContinued:
                    var requeueCmd = new RequeueDocumentCommand(
                        evt.ReferenceNumber, 
                        evt.Priority, 
                        ImportSource.Uspto,
                        true, 
                        evt.RetryCount);

                    var requeueResult = await _mediator.Send(requeueCmd, cancellationToken).ConfigureAwait(false);

                    break;
                default:
                    _logger.LogError($"Unknown fetch status result. {nameof(PAIRDocumentIntegrationEvent)}:{evt.ToJson()}.");
                    break;
            }

            return;
        }
    }
}