using Patents.ArtRepoCloud.Domain.IntegrationEvents;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Vikcher.Framework.Common;
using MediatR;
using PDFtoImage;
using SkiaSharp;

namespace Patents.ArtRepoCloud.Service.Application.IntegrationEventHandlers
{
    public class ImageExtractionIntegrationEventHandler : INotificationHandler<ImageExtractionIntegrationEvent>
    {
        private readonly IFileRepository _fileRepository;
        private readonly ILogger<ImageExtractionIntegrationEventHandler> _logger;

        public ImageExtractionIntegrationEventHandler(IFileRepository fileRepository, ILogger<ImageExtractionIntegrationEventHandler> logger)
        {
            _fileRepository = fileRepository;
            _logger = logger;
        }

        public async Task Handle(ImageExtractionIntegrationEvent evt, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Handling {nameof(ImageExtractionIntegrationEvent)}: {evt.ToJson()}.");

            await using var pdfStream =
                await _fileRepository.GetAsync(evt.SourcePath, cancellationToken).ConfigureAwait(false);

            if (pdfStream != null)
            {
                var images = Conversion.ToImages(pdfStream).ToList();

                var i = 0;
                foreach (var image in images)
                {
                    i++;
                    if (evt.Pages?.Contains(i) ?? true)
                    {
                        using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 1))
                        using (MemoryStream stream = new MemoryStream())
                        {
                            data.SaveTo(stream);

                            stream.Position = 0;

                            await _fileRepository.SaveAsync(
                                stream,
                                evt.DestinationPath,
                                $"{i}.jpeg",
                                cancellationToken);
                        }

                        image.Dispose();
                    }
                }
            }
        }
    }
}