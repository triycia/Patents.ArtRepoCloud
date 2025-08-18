using Microsoft.Azure.Amqp;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Patents.ArtRepoCloud.Service.Code.Extensions
{
    public static class Extentions
    {
        public static MessageReceiver GetMessageReceiver(this QueueClient obj)
        {
            var fieldInfo0 = obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);


            var fieldInfo = obj.GetType().GetField("innerReceiver", BindingFlags.NonPublic | BindingFlags.Instance);

            var reader = fieldInfo?.GetValue(obj) as MessageReceiver;

            return reader;
        }

        public static dynamic GetReceivePump(this MessageReceiver obj)
        {
            var fieldInfo = obj.GetType().GetField("receivePump", BindingFlags.NonPublic | BindingFlags.Instance);

            var reader = fieldInfo?.GetValue(obj);

            return reader;
        }

        public static CancellationTokenSource GetCancellationToken(this MessageReceiver obj)
        {
            var fieldInfo = obj.GetType().GetField("receivePumpCancellationTokenSource", BindingFlags.NonPublic | BindingFlags.Instance);

            var reader = fieldInfo?.GetValue(obj) as CancellationTokenSource;

            return reader;
        }

        public static bool SetCancellationToken(this MessageReceiver obj, Func<CancellationTokenSource, CancellationTokenSource> action)
        {
            var fieldInfo = obj.GetType().GetField("receivePumpCancellationTokenSource", BindingFlags.NonPublic | BindingFlags.Instance);

            var token = fieldInfo?.GetValue(obj) as CancellationTokenSource;

            token = action(token);

            fieldInfo?.SetValue(obj, token);

            return true;
        }

        public static T GetField<T>(this object obj, string fieldName)
        {
            var fieldInfo = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            var field = (T)fieldInfo?.GetValue(obj)!;

            return field;
        }

        public static dynamic GetField(this object obj, string fieldName)
        {
            var fieldInfo = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            var field = fieldInfo?.GetValue(obj);

            return field;
        }
    }
}