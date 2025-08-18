using Vikcher.Framework.EventBus.Events;
using MediatR;

namespace Patents.ArtRepoCloud.Domain.IntegrationEvents
{
    public class ImageExtractionIntegrationEvent : IntegrationEvent, INotification
    {
        public ImageExtractionIntegrationEvent(string sourcePath, string destinationPath, int[]? pages)
        {
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
            Pages = pages;
        }

        public string SourcePath { get; }
        public string DestinationPath { get; }
        public int[]? Pages { get; }
    }
}