using Vikcher.Framework.Common;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Domain.IntegrationEvents;
using Patents.ArtRepoCloud.Service.Application.Commands.EnqueueFamily;
using Patents.ArtRepoCloud.Service.Application.Commands.EnqueueImages;
using Patents.ArtRepoCloud.Service.Application.Commands.EnqueuePdf;
using Patents.ArtRepoCloud.Service.Application.Commands.RequeueDocument;
using Patents.ArtRepoCloud.Service.Configuration;
using Patents.ArtRepoCloud.Service.DataFetchers;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchRequests;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses;
using Patents.ArtRepoCloud.Service.Enums;
using Patents.ArtRepoCloud.Service.Extensions;
using MediatR;
using Patents.ArtRepoCloud.Service.Application.Commands.ImportStatus;
using Patents.ArtRepoCloud.Service.Application.Commands.SaveArtRepoDocument;
using System.IdentityModel.Tokens.Jwt;
using Patents.ArtRepoCloud.Service.DataProviders.Ifi.Interfaces;
using Autofac;
using Patents.ArtRepoCloud.Domain.Extensions;
using Patents.ArtRepoCloud.Domain.Factories.Interfaces;
using Patents.ArtRepoCloud.Domain.ReferenceNumbers;
using Patents.ArtRepoCloud.Domain.Code.Interfaces;
using Patents.ArtRepoCloud.Domain.Code;

namespace Patents.ArtRepoCloud.Service.Application.IntegrationEventHandlers
{
    public class ArtRepoDocumentIntegrationEventHandler : INotificationHandler<ArtRepoDocumentIntegrationEvent>
    {
        private readonly IReferenceNumberParser _referenceNumberParser;
        private readonly IMediator _mediator;
        private readonly IIfiApiProxy _apiProxy;
        private readonly IFetcher<DocumentFetchRequest, DocumentFetchResponse>[] _fetchers;
        private readonly AppSettings _appSettings;
        private readonly ILogger<ArtRepoDocumentIntegrationEventHandler> _logger;

        public ArtRepoDocumentIntegrationEventHandler(IReferenceNumberParser referenceNumberParser, IMediator mediator, IIfiApiProxy apiProxy, IFetcher<DocumentFetchRequest, DocumentFetchResponse>[] fetchers, AppSettings appSettings, ILogger<ArtRepoDocumentIntegrationEventHandler> logger)
        {
            _referenceNumberParser = referenceNumberParser;
            _mediator = mediator;
            _apiProxy = apiProxy;
            _fetchers = fetchers;
            _appSettings = appSettings;
            _logger = logger;
        }

        public async Task Handle(ArtRepoDocumentIntegrationEvent evt, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Handling integration event {nameof(ArtRepoDocumentIntegrationEvent)}:{evt.ToJson()}.");

            var referenceNumber = evt.ReferenceNumber;

            await _mediator.Send(new ImportStatusCommand(referenceNumber, QueueStatus.InProcess), cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrEmpty(referenceNumber.KindCode) && _fetchers.All(x => !x.IsTypeOf(ImportSource.Ifi)))
            {
                var ucid = await _apiProxy.GetUcidAsync(referenceNumber, cancellationToken).ConfigureAwait(false);
                var fromUcidNumber = _referenceNumberParser.Parse(ucid);

                if (!string.IsNullOrEmpty(fromUcidNumber?.KindCode))
                {
                    referenceNumber.SetKindCode(fromUcidNumber.KindCode);
                }
            }

            foreach (var fetcher in _fetchers.StartAt(evt.ImportSource, evt.UseSourceStrictly))
            {
                if (fetcher.IsTypeOf(ImportSource.Uspto) && !referenceNumber.IsUs())
                {
                    continue;
                }

                var response = await fetcher.ProcessAsync(new DocumentFetchRequest(referenceNumber), cancellationToken).ConfigureAwait(false);

                switch (response.HttpReasonCode)
                {
                    case HttpReasonCode.None:
                        throw new InvalidOperationException("System error. The data fetch cannot be started.");
                    case HttpReasonCode.Failed:
                        await _mediator.Send(new ImportStatusCommand(referenceNumber, QueueStatus.Failed), cancellationToken).ConfigureAwait(false);

                        var msg =
                            $"This document data download has failed at {evt.RetryCount} attempt on {response.Source.GetName()} data source. Throwing an exception to put the Queue to Dead-letter.";

                        _logger.LogWarning(msg);

                        throw new Exception(msg);
                    case HttpReasonCode.Success:
                        var saveDocCmd = new SaveArtRepoDocumentCommand(response.ReferenceNumber, response.Document!, response.Source.ToDataSource(), evt.IsUserImport);

                        await _mediator.Send(saveDocCmd, cancellationToken).ConfigureAwait(false);

                        var enqueuePdfCmd = new EnqueuePdfCommand(
                            response.ReferenceNumber, 
                            evt.Priority, 
                            response.Source, 
                            evt.UseSourceStrictly);

                        await _mediator.Send(enqueuePdfCmd, cancellationToken).ConfigureAwait(false);

                        var enqueueImagesCmd = new EnqueueImagesCommand(
                            response.ReferenceNumber,
                            evt.Priority.Equals(ImportPriority.Idle) 
                                ? ImportPriority.Idle 
                                : ImportPriority.Low, 
                            response.Source, 
                            evt.UseSourceStrictly);

                        await _mediator.Send(enqueueImagesCmd, cancellationToken).ConfigureAwait(false);

                        if (_appSettings.EnableFamilyImport && response.Document!.FamilySimpleList.Any() && evt.IsUserImport)
                        {
                            var simpleFamilyCmd = new EnqueueFamilyCommand(
                                response.ReferenceNumber, 
                                response.Document.FamilySimpleList
                                    .Select(f => f.Replace("-", "")).ToList());

                            await _mediator.Send(simpleFamilyCmd, cancellationToken).ConfigureAwait(false);
                        }

                        if (_appSettings.EnableExtendedFamilyImport && response.Document!.FamilyExtendedList.Any() && evt.IsUserImport)
                        {
                            var simpleFamilyCmd = new EnqueueFamilyCommand(
                                response.ReferenceNumber, 
                                response.Document.FamilyExtendedList
                                    .Select(f => f.Replace("-", "")).ToList());

                            await _mediator.Send(simpleFamilyCmd, cancellationToken).ConfigureAwait(false);
                        }

                        return;
                    case HttpReasonCode.NoData:
                        if (Array.IndexOf(_fetchers, fetcher) < _fetchers.Length - 1)
                        {
                            continue;
                        }

                        await _mediator.Send(new ImportStatusCommand(referenceNumber, QueueStatus.NoData), cancellationToken).ConfigureAwait(false);

                        _logger.LogInformation($"No document data found on {response.Source.GetName()} data source for Reference # \"{referenceNumber}\". The process has ended.");

                        break;
                    case HttpReasonCode.ToBeContinued:
                        await _mediator.Send(new ImportStatusCommand(referenceNumber, QueueStatus.ToBeContinued), cancellationToken).ConfigureAwait(false);

                        var requeueCmd = new RequeueDocumentCommand(
                            referenceNumber, 
                            evt.Priority, 
                            ImportSource.All,
                            evt.IsUserImport,
                            (byte)(evt.RetryCount + 1));
                        await _mediator.Send(requeueCmd, cancellationToken).ConfigureAwait(false);

                        break;
                    case HttpReasonCode.FetchSourceUnavailable:
                        await _mediator.Send(new ImportStatusCommand(referenceNumber, QueueStatus.ToBeContinued), cancellationToken).ConfigureAwait(false);

                        break;
                    default:
                        _logger.LogError($"Unknown fetch status result. {nameof(ArtRepoDocumentIntegrationEvent)}:{evt.ToJson()}.");
                        break;
                }

                return;
            }
        }
    }
}