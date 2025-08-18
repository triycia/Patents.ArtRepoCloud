using System.Reflection;
using Patents.ArtRepoCloud.Domain.Attributes;
using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Service.Code.Attributes;

namespace Patents.ArtRepoCloud.Service.Extensions
{
    public static class Extensions
    {
        public static string Format(this string urlStr, string pathParameterName, string pathParameter)
        {
            return urlStr.Replace("{" + pathParameterName + "}", pathParameter);
        }

        public static void Format(this string urlStr, params string[] parameters)
        {
            Console.WriteLine(parameters);
            foreach (string number in parameters)
                Console.WriteLine(number);
        }

        public static TAttribute? GetAttribute<TAttribute>(this Enum enumVal)
        {
            var memberInfo = enumVal.GetType().GetMember(enumVal.ToString());

            return memberInfo[0].GetCustomAttributes(typeof(TAttribute), false).OfType<TAttribute>().FirstOrDefault();
        }

        public static ImportSource GetImportSource(this object obj)
        {
            var attr = obj.GetType().GetCustomAttribute(typeof(ImportSourceAttribute), true) as ImportSourceAttribute;

            return attr.Source;
        }

        public static (ImportSource Source, ImportPriority Priority) GetQueueClientType<TEventBusQueueSettings>(this TEventBusQueueSettings obj)
        {
            var attribute = obj.GetType().GetCustomAttribute(typeof(QueueClientTypeAttribute), true);

            var attr = attribute as QueueClientTypeAttribute;

            return (attr.Source, attr.Priority);
        }

        public static DataSource ToDataSource(this ImportSource source)
        {
            return Enum.GetValues(typeof(DataSource))
                .Cast<DataSource>()
                .Select(x => new { name = x.GetName(), value = x })
                .FirstOrDefault(x => x.name == source.GetName())?.value ?? DataSource.Unknown;
        }

        public static string? GetName<T>(this T enumVal) where T : Enum
        {
            return Enum.GetName(typeof(T), enumVal);
        }
    }
}