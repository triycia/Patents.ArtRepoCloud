using System.Net.Http.Headers;

namespace Patents.ArtRepoCloud.Service.Extensions
{
    public static class HttpExtensions
    {
        public static string GetValue(this HttpResponseHeaders headers, string key)
        {
            IEnumerable<string> values;

            return headers.TryGetValues(key, out values) 
                ? values.First() 
                : string.Empty;
        }
        public static T TryGetValue<T>(this HttpResponseHeaders headers, string key)
        {
            IEnumerable<string> values;

            var result = headers.TryGetValues(key, out values)
                ? values.First()
                : string.Empty;

            return result.TryConvertTo(typeof(T));
        }

        public static dynamic TryConvertTo(this object source, Type dest)
        {
            return Convert.ChangeType(source, dest);
        }
    }
}