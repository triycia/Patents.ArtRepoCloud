using Autofac;
using Vikcher.Framework.EventBus.Abstractions.Queue;
using Patents.ArtRepoCloud.Domain.Attributes;
using Patents.ArtRepoCloud.Domain.Enums;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace Patents.ArtRepoCloud.Domain.Extensions
{
    public static class Extensions
    {
        public static bool Compare(this string? strOne, string? strTwo)
        {
            strOne = strOne?.Trim();
            strTwo = strTwo?.Trim();

            if (string.IsNullOrEmpty(strOne) || string.IsNullOrEmpty(strTwo))
            {
                return strOne == strTwo;
            }

            var resOne = Regex.Replace(strOne, @"\s\s+", " ");
            var resTwo = Regex.Replace(strTwo, @"\s\s+", " ");

            return resOne.Equals(resTwo, StringComparison.CurrentCultureIgnoreCase);
        }

        public static dynamic ResolvePatentQueueClient(this ILifetimeScope scope, ImportPriority importPriority)
        {
            return scope.ResolveQueueClient(importPriority, ImportSource.All);
        }

        public static dynamic ResolveQueueClient(this ILifetimeScope scope, ImportPriority importPriority, ImportSource source)
        {
            var clientType = AppDomain.CurrentDomain.GetAssemblies()
                       .SelectMany(t => t.GetTypes())
                       .FirstOrDefault(t => t.IsClass 
                                            && t.GetInterface(nameof(IEventBusQueueClientSettings)) != null
                                            && t.GetCustomAttribute(typeof(QueueClientTypeAttribute), false) is QueueClientTypeAttribute attr
                                            && attr.TypeOf(importPriority, source));
            if (clientType != null)
            {
                Type genericType = typeof(IEventBusQueueClient<>).MakeGenericType(clientType);

                return scope.Resolve(genericType);
            }

            throw new ArgumentOutOfRangeException(
                $"No Queue found with {nameof(ImportSource)}: {source} and {nameof(ImportPriority)}: {importPriority}.");
        }

        public static T? ReadAs<T>(this Stream? stream)
        {
            if (stream != null)
            {
                using StreamReader reader = new StreamReader(stream);
                var strRes = reader.ReadToEnd();

                var str = JsonConvert.DeserializeObject(strRes)?.ToString();

                return str != null
                    ? JsonConvert.DeserializeObject<T>(str)
                    : default;
            }

            return default;
        }

        public static string CalculateHash(this string[] identifiers)
        {
            var sortedIds = identifiers.OrderBy(i => i);
            var result = string.Join("_", sortedIds.ToArray());
            var md5Hasher = MD5.Create();
            var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(result));
            var builder = new StringBuilder();

            foreach (var t in data)
            {
                builder.Append(t.ToString("x2"));
            }

            return builder.ToString();
        }

        public static DocumentType GetDocumentType(this ReferenceNumberSourceType enumVal)
        {
            var memberInfo = enumVal.GetType().GetMember(enumVal.ToString());

            return memberInfo[0].GetCustomAttributes(typeof(DocumentTypeAttribute), false)
                .OfType<DocumentTypeAttribute>().FirstOrDefault()?.DocumentType ?? DocumentType.Unknown;
        }
    }
}