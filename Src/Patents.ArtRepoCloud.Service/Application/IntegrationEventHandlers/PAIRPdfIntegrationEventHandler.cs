using Patents.ArtRepoCloud.Domain.IntegrationEvents;
using Patents.ArtRepoCloud.Service.Configuration;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchRequests;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses;
using Patents.ArtRepoCloud.Service.DataFetchers;
using MediatR;
using Patents.ArtRepoCloud.Service.Enums;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Service.Application.Commands.EnqueuePdf;
using Patents.ArtRepoCloud.Service.Extensions;
using Patents.ArtRepoCloud.Service.Application.Commands.RequeueDocument;
using Patents.ArtRepoCloud.Service.Application.Commands.SavePairDocumentPdfs;
using Patents.ArtRepoCloud.Service.Application.Queries.ArtRepoDocument;
using Vikcher.Framework.Common;

namespace Patents.ArtRepoCloud.Service.Application.IntegrationEventHandlers
{
    internal class PAIRPdfIntegrationEventHandler : INotificationHandler<PAIRPdfIntegrationEvent>
    {
        private readonly IMediator _mediator;
        private readonly IFetcher<PairPdfFetchRequest, PairPdfFetchResponse> _fetcher;
        private readonly AppSettings _appSettings;
        private readonly ILogger<ArtRepoDocumentIntegrationEventHandler> _logger;

        public PAIRPdfIntegrationEventHandler(IMediator mediator, IFetcher<PairPdfFetchRequest, PairPdfFetchResponse> fetcher, AppSettings appSettings, ILogger<ArtRepoDocumentIntegrationEventHandler> logger)
        {
            _mediator = mediator;
            _fetcher = fetcher;
            _appSettings = appSettings;
            _logger = logger;
        }

        public async Task Handle(PAIRPdfIntegrationEvent evt, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Handling integration event {nameof(PAIRPdfIntegrationEvent)} for reference {evt.ReferenceNumber}.");

            var documentResult = await _mediator.Send(new ArtRepoDocumentQuery(evt.ReferenceNumber), cancellationToken).ConfigureAwait(false);

            if (documentResult.Document == null)
            {
                _logger.LogWarning($"{nameof(PAIRPdfIntegrationEvent)} have failed for reference {evt.ReferenceNumber}. Document not found in our system!");

                throw new InvalidOperationException(
                    $"{nameof(PAIRPdfIntegrationEvent)} have failed for reference {evt.ReferenceNumber}. Document not found in our system!");
            }

            var pairData = documentResult.Document.PairData;

            if (pairData == null)
            {
                _logger.LogWarning($"{nameof(PAIRPdfIntegrationEvent)} have failed for reference {evt.ReferenceNumber}. Document PairData not found in our system!");

                throw new InvalidOperationException(
                    $"{nameof(PAIRPdfIntegrationEvent)} have failed for reference {evt.ReferenceNumber}. Document PairData not found in our system!");
            }

            var response = await _fetcher.ProcessAsync(new PairPdfFetchRequest(evt.ReferenceNumber, pairData.CustomerNumber, pairData.PairFiles), cancellationToken).ConfigureAwait(false);

            switch (response.HttpReasonCode)
            {
                case HttpReasonCode.None:
                    throw new InvalidOperationException("System error. The pdf fetch cannot be started.");
                case HttpReasonCode.Failed:
                    throw new Exception($"This {ImportSource.Uspto.GetName()} pdf attempt has failed on {evt.RetryCount} attempt on {response.Source.GetName()}. Throwing an exception to put the Queue to Dead-letter.");
                case HttpReasonCode.NoData:
                    _logger.LogInformation($"No pdf found for \"{evt.ReferenceNumber}\". The process has ended.");

                    break;
                case HttpReasonCode.Success:
                    var saveImagesCmd = new SavePairDocumentPdfsCommand(evt.ReferenceNumber, response.PdfFiles);

                    await _mediator.Send(saveImagesCmd, cancellationToken).ConfigureAwait(false);

                    break;
                case HttpReasonCode.ToBeContinued:
                    var enqueuePdfCmd = new EnqueuePdfCommand(
                        response.ReferenceNumber,
                        evt.Priority,
                        response.Source,
                        true,
                        (byte)(evt.RetryCount + 1));

                    await _mediator.Send(enqueuePdfCmd, cancellationToken).ConfigureAwait(false);

                    break;
                default:
                    _logger.LogError($"Unknown fetch status result. {nameof(PAIRPdfIntegrationEvent)}:{evt.ToJson()}.");
                    break;
            }
        }
    }
}