using Autofac;
using Vikcher.Framework.EventBus.Abstractions.Queue;
using Vikcher.Framework.Logging;
using Patents.ArtRepoCloud.Service.EventBusSettings;
using Patents.ArtRepoCloud.Service.Code.AzureServiceBusQueue.Interfaces;

namespace Patents.ArtRepoCloud.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly ILoggerAdapter<Worker> _logger;

        public Worker(ILifetimeScope lifetimeScope, ILoggerAdapter<Worker> logger)
        {
            _lifetimeScope = lifetimeScope;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Info($"Service Bus started at: {DateTimeOffset.Now}");

            await RegisterServiceBusQueues();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
            }

            _logger.Info($"Service Bus stopped at: {DateTimeOffset.Now}");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Info("Stopping Patents Service.");

            await DisposeServiceBusQueues();


            await base.StopAsync(cancellationToken).ConfigureAwait(false);

            _logger.Info("Stopped Patents Service.");
        }

        internal async Task DisposeServiceBusQueues()
        {
            await using var scope = _lifetimeScope.BeginLifetimeScope();
            scope.Resolve<IPriorityEventBusQueue<ArtRepoServiceBusHighPriorityQueueSettings>>().Dispose();
            scope.Resolve<IPriorityEventBusQueue<ArtRepoServiceBusNormalPriorityQueueSettings>>().Dispose();
            scope.Resolve<IPriorityEventBusQueue<ArtRepoServiceBusLowPriorityQueueSettings>>().Dispose();
            scope.Resolve<IPriorityEventBusQueue<ArtRepoServiceBusIdlePriorityQueueSettings>>().Dispose();

            //scope.Resolve<IPriorityEventBusQueue<PAIRServiceBusHighPriorityQueueSettings>>().Dispose();
            //scope.Resolve<IPriorityEventBusQueue<PAIRServiceBusNormalPriorityQueueSettings>>().Dispose();
            //scope.Resolve<IPriorityEventBusQueue<PAIRServiceBusLowPriorityQueueSettings>>().Dispose();
            //scope.Resolve<IPriorityEventBusQueue<PAIRServiceBusIdlePriorityQueueSettings>>().Dispose();

            scope.Resolve<IEventBusQueue<ArtRepoFileServiceBusQueueSettings>>().Dispose();
        }

        internal async Task RegisterServiceBusQueues()
        {
            await using var scope = _lifetimeScope.BeginLifetimeScope();
            scope.Resolve<IPriorityEventBusQueue<ArtRepoServiceBusHighPriorityQueueSettings>>().Subscribe();
            scope.Resolve<IPriorityEventBusQueue<ArtRepoServiceBusNormalPriorityQueueSettings>>().Subscribe();
            scope.Resolve<IPriorityEventBusQueue<ArtRepoServiceBusLowPriorityQueueSettings>>().Subscribe();
            scope.Resolve<IPriorityEventBusQueue<ArtRepoServiceBusIdlePriorityQueueSettings>>().Subscribe();

            //scope.Resolve<IPriorityEventBusQueue<PAIRServiceBusHighPriorityQueueSettings>>().Subscribe();
            //scope.Resolve<IPriorityEventBusQueue<PAIRServiceBusNormalPriorityQueueSettings>>().Subscribe();
            //scope.Resolve<IPriorityEventBusQueue<PAIRServiceBusLowPriorityQueueSettings>>().Subscribe();
            //scope.Resolve<IPriorityEventBusQueue<PAIRServiceBusIdlePriorityQueueSettings>>().Subscribe();

            scope.Resolve<IEventBusQueue<ArtRepoFileServiceBusQueueSettings>>().Subscribe();
        }
    }
}