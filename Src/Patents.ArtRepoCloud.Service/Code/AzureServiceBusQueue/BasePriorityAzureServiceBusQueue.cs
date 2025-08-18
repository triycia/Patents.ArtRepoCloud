using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;
using Vikcher.Framework.EventBus.Abstractions.Queue;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Service.Code.Extensions;
using Microsoft.Azure.ServiceBus;

namespace Patents.ArtRepoCloud.Service.Code.AzureServiceBusQueue
{
    public abstract class BasePriorityAzureServiceBusQueue
    {
        protected static readonly int QueuesCount = Enum.GetValues<ImportPriority>().Count();

        protected static Dictionary<ImportSource, IList<(IEventBusQueueSettings Settings, QueueClient Client)>> QueueClientsDict = new() { { ImportSource.All, new List<(IEventBusQueueSettings, QueueClient)>() }, { ImportSource.Uspto, new List<(IEventBusQueueSettings, QueueClient)>() } };

        protected void AddQueueClient(ImportSource source, int priorityIndex, IEventBusQueueSettings settings, QueueClient? client)
        {
            IList<(IEventBusQueueSettings, QueueClient)> clients = QueueClientsDict[source];
            clients.Insert(priorityIndex, (settings, client));
        }

        protected IList<(IEventBusQueueSettings Settings, QueueClient Client)> GetQueueClients(ImportSource source)
        {
            IList<(IEventBusQueueSettings, QueueClient)> clients = QueueClientsDict[source];
            return clients;
        }

        protected async Task StopDescendingQueues(QueueClient? currentClient, ImportSource importSource, int priorityIndex)
        {
            var clients = GetQueueClients(importSource);

            for (int i = priorityIndex + 1; i < clients.Count; i++)
            {
                var client = clients[i];

                await client.Client.UnregisterMessageHandlerAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false); //.RunSynchronously();

                var innerReceiverField = client.Client?.GetType().GetField("innerReceiver", BindingFlags.NonPublic | BindingFlags.Instance);

                innerReceiverField?.SetValue(client.Client, null);
            }
        }

        protected void StartLowerQueue(QueueClient currentClient, ImportSource importSource, int priorityIndex)
        {
            var clients = GetQueueClients(importSource);

            for (int i = priorityIndex + 1; i < clients.Count; i++)
            {
                var client = clients[i];

                var messageReceiver = client.Client.GetMessageReceiver();

                var receivePump = messageReceiver.GetReceivePump();

                var token = messageReceiver.GetCancellationToken();

                messageReceiver.SetCancellationToken(token =>
                {
                    if (token.IsCancellationRequested)
                    {
                        return new CancellationTokenSource();
                    }

                    return token;
                });

                receivePump.StartPump();
            }
        }
    }
}