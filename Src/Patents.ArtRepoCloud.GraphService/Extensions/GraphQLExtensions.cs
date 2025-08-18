using Patents.ArtRepoCloud.GraphService.Code.GraphQL;
using HotChocolate.Data.Projections.Context;
using HotChocolate.Language;
using HotChocolate.Resolvers;

namespace Patents.ArtRepoCloud.GraphService.Extensions
{
    public static class GraphQLExtensions
    {
        public static IApplicationBuilder UseGraphQLMultipartForm(this IApplicationBuilder app)
        {
            return app.UseMiddleware<MultipartRequestMiddleware>();
        }

        public static IEnumerable<string> GetSelections(this IResolverContext context)
        {
            return context.GetSelectedField()
                .Selection
                .SelectionSet
                ?.Selections
                .Select(x => ((FieldNode)x).Name.Value.ToLower()) ?? Enumerable.Empty<string>();
        }
    }
}