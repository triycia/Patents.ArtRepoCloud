using Autofac;
using Vikcher.Framework.EventBus.Abstractions.Queue;
using Vikcher.Framework.EventBus.AzureServiceBus.Queue;
using Vikcher.Framework.Logging;
using MediatR;
using Microsoft.Azure.ServiceBus.Primitives;
using Microsoft.Azure.ServiceBus;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Azure.Messaging.ServiceBus.Administration;
using Patents.ArtRepoCloud.Service.Code.Extensions;
using Microsoft.Azure.ServiceBus.Core;
using Patents.ArtRepoCloud.Service.Configuration;
using Patents.ArtRepoCloud.Service.Extensions;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Service.EventBusSettings.Interfaces;
using Patents.ArtRepoCloud.Service.Code.AzureServiceBusQueue.Interfaces;

namespace Patents.ArtRepoCloud.Service.Code.AzureServiceBusQueue
{
    public class ArtRepoAzureServiceBusQueue<TEventBusQueueSettings> : BasePriorityAzureServiceBusQueue, IPriorityEventBusQueue<TEventBusQueueSettings> where TEventBusQueueSettings : IPriorityEventBusQueueSettings
    {
        private readonly ILoggerAdapter<DefaultAzureServiceBusQueue<TEventBusQueueSettings>> _logger;
        private QueueClient? _queueClient;
        private readonly TEventBusQueueSettings _eventBusQueueSettings;
        private readonly ILifetimeScope _lifetimeScope;
        private readonly ServiceBusAdministrationClient _administrationClient;
        private readonly AppSettings _appSettings;
        private readonly ImportSource _importSource;
        private readonly ImportPriority _importPriority;
        private int _priorityIndex;

        public ArtRepoAzureServiceBusQueue(
            TEventBusQueueSettings eventBusQueueSettings,
            ILifetimeScope lifetimeScope, 
            AppSettings appSettings, 
            ILoggerAdapter<DefaultAzureServiceBusQueue<TEventBusQueueSettings>> logger)
        {
            _lifetimeScope = lifetimeScope;
            _appSettings = appSettings;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _eventBusQueueSettings = eventBusQueueSettings;
            _administrationClient = new ServiceBusAdministrationClient(_eventBusQueueSettings.ConnectionString);
            var clientType = _eventBusQueueSettings.GetQueueClientType();

            _importSource = clientType.Source;
            _importPriority = clientType.Priority;
        }

        public void Subscribe()
        {
            if (string.IsNullOrWhiteSpace(_eventBusQueueSettings.ConnectionString))
            {
                throw new InvalidOperationException("The connectionString is required to create a queue client.");
            }

            if (string.IsNullOrWhiteSpace(_eventBusQueueSettings.QueueName))
            {
                throw new InvalidOperationException("The QueueName is required to create a queue client.");
            }

            _queueClient = _eventBusQueueSettings.UseManagedIdentity
                ? new QueueClient(_eventBusQueueSettings.ConnectionString,
                    _eventBusQueueSettings.QueueName, new ManagedIdentityTokenProvider(),
                    TransportType.Amqp, ReceiveMode.PeekLock, RetryPolicy.Default)
                : new QueueClient(_eventBusQueueSettings.ConnectionString,
                    _eventBusQueueSettings.QueueName, ReceiveMode.PeekLock, RetryPolicy.Default);

            AddQueueClient(_importSource, (int)_importPriority, _eventBusQueueSettings, _queueClient);

            if (_priorityIndex > 0)
            {
                var clients = GetQueueClients(_importSource);

                var cl = clients.Select(x => x.Settings)
                    .Select((q, i) => new { i, q })
                    .Where(x => x.i < _priorityIndex);

                var isHigherPriorityPending = cl.Any(x => GetPendingCount(x.q.QueueName) > 0);

                if (!isHigherPriorityPending)
                {
                    RegisterQueueClientMessageHandler(_queueClient, _eventBusQueueSettings.QueueName, _importSource, (int)_importPriority);
                }
            }
            else 
            {
                RegisterQueueClientMessageHandler(_queueClient, _eventBusQueueSettings.QueueName, _importSource, (int)_importPriority);
            }
        }

        private long GetPendingCount(string queueName)
        {
            return _administrationClient.GetQueueRuntimePropertiesAsync(queueName).Result.Value.ActiveMessageCount;
        }

        protected void StartDescendingQueue(QueueClient? currentClient, ImportSource importSource, int priorityIndex, Action<QueueClient, string, ImportSource, int> action)
        {
            var clients = GetQueueClients(importSource);

            for (int i = priorityIndex + 1; i < clients.Count; i++)
            {
                var client = clients[i];
                var clientSettings = client.Settings;

                action(client.Client, client.Settings.QueueName, importSource, priorityIndex);

                var pendingCount = GetPendingCount(client.Settings.QueueName);

                if (pendingCount > 0)
                {
                    break;
                }
            }
        }

        private QueueClient? GetNextQueueClient(ImportSource importSource, int priorityIndex)
        {
            var clients = GetQueueClients(importSource);

            for (int i = priorityIndex + 1; i < clients.Count; i++)
            {
                var client = clients[i];

                return client.Client;
            }

            return null;
        }

        private void RegisterQueueClientMessageHandler(QueueClient? queueClient, string queueName, ImportSource importSource, int priorityIndex)
        {
            var clients = GetQueueClients(importSource);

            var HasDescending = () => clients.Count > priorityIndex + 1;

            queueClient.RegisterMessageHandler(
                async (message, cancellationToken) =>
                {
                    var eventName = message.Label;
                    var messageData = Encoding.UTF8.GetString(message.Body);

                    // Complete the message so that it is not received again.
                    if (await ProcessEvent(eventName, messageData, cancellationToken).ConfigureAwait(false))
                    {
                        await queueClient.CompleteAsync(message.SystemProperties.LockToken).ConfigureAwait(false);

                        var messageCount = GetPendingCount(queueName);

                        Console.WriteLine($"Queue Name: {queueName}. Pending Count: {messageCount}.");

                        var nextClient = GetNextQueueClient(importSource, priorityIndex);
                        var receiver = nextClient?.GetField<MessageReceiver>("innerReceiver");

                        if (messageCount <= 0 && nextClient != null && receiver == null)
                        {
                            StartDescendingQueue(queueClient, importSource, priorityIndex, RegisterQueueClientMessageHandler);
                        }
                        else if (HasDescending() && messageCount > 10 && receiver != null)
                        {
                            await StopDescendingQueues(queueClient, importSource, priorityIndex).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        //////
                    }
                },
                new MessageHandlerOptions(ExceptionReceivedHandler) { MaxConcurrentCalls = _appSettings.MaxConcurrentCalls, AutoComplete = false });

            //_logger.Info($"Subscribed queue: {queueName}.");
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var ex = exceptionReceivedEventArgs.Exception;
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;

            _logger.Error(ex,
                $"ERROR handling message: {ex.Message} - Endpoint: {context.Endpoint} - Entity RelativePath: {context.EntityPath} - Executing Action: {context.Action}");
            return Task.CompletedTask;
        }

        private async Task<bool> ProcessEvent(string eventName, string message, CancellationToken cancellationToken)
        {
            //_logger.Info($"Processing the queue message: {eventName}.");

            var assembly = Assembly.Load(_eventBusQueueSettings.EventAssemblyName);
            var messageType = assembly.GetType($"{_eventBusQueueSettings.EventNamespace}.{eventName}");

            if (messageType == null)
            {
                throw new InvalidOperationException($"No type found for {eventName}.");
            }

            var messageObject = JsonConvert.DeserializeObject(message, messageType);

            if (messageObject == null)
            {
                throw new InvalidOperationException($"Failed to deserialize object for type {messageType} with json {message}.");
            }

            if (messageObject is INotification notification)
            {
                using (var scope = _lifetimeScope.BeginLifetimeScope())
                {
                    await scope.Resolve<IMediator>().Publish(notification, cancellationToken).ConfigureAwait(false);
                    //_logger.Info($"published notification {eventName} via MediatorR.");
                }
            }

            return true;
        }

        public void Dispose()
        {
            _logger.Info($"Closing QueueClient {_eventBusQueueSettings.QueueName}.");

            Task.Run(async () => await _queueClient.CloseAsync().ConfigureAwait(false));
        }
    }
}