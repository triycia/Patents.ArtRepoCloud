using Patents.ArtRepoCloud.Domain.Enums;
using Patents.ArtRepoCloud.Service.Code.Attributes;
using Patents.ArtRepoCloud.Service.DataFetchers;
using System.Reflection;

namespace Patents.ArtRepoCloud.Service.Extensions
{
    public static class FetcherExtensions
    {
        public static IEnumerable<IFetcher<TRequest, TResult>> StartAt<TRequest, TResult>(this IFetcher<TRequest, TResult>[] array, ImportSource source, bool fetchStrictly)
        {
            if (source.Equals(ImportSource.All))
            {
                return array;
            }

            if (fetchStrictly)
            {
                return array.Where(fetcher => fetcher.IsTypeOf(source)).ToArray();
            }

            var result = new List<IFetcher<TRequest, TResult>>();

            foreach (var item in array)
            {
                if (item.IsTypeOf(source) || result.Any())
                {
                    result.Add(item);
                }
            }

            return result.ToArray();
        }

        public static bool IsTypeOf<TRequest, TResult>(this IFetcher<TRequest, TResult> obj, ImportSource source)
        {
            Type type = obj.GetType();

            var attr = type.GetCustomAttribute(typeof(ImportSourceAttribute), true) as ImportSourceAttribute;

            return attr.Source.Equals(source);
        }
    }
}
