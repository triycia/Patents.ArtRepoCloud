using System.Text;

namespace Patents.ArtRepoCloud.GraphService.Code.GraphQL
{
    public class MultipartRequestMiddleware
    {
        private const string _operations = "operations";

        private readonly RequestDelegate _next;
        public MultipartRequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.Value != "/graphql/" || !context.Request.HasFormContentType)
            {
                await _next(context);
            }
            else
            {
                var parsedOperationsStr = context.Request.Form[_operations];

                context.Request.ContentType = "application/json";
                context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(parsedOperationsStr));
                await _next(context);
            }
        }
    }
}