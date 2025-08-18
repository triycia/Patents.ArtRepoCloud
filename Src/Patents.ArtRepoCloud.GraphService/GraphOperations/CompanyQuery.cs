using Autofac;
using Patents.ArtRepoCloud.Domain.Aggregates.CompanyAggregate;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Patents.ArtRepoCloud.GraphService.Code.GraphQL.GraphQLTypes.ArtRepoCompanyTypes;
using Vikcher.Framework.Data.EntityFramework.Queryable;
using HotChocolate.Data.Projections.Context;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using Microsoft.EntityFrameworkCore;

namespace Patents.ArtRepoCloud.GraphService.GraphOperations
{
    [ExtendObjectType("Query")]
    public class CompanyQuery
    {
        [UseProjection]
        [UseFiltering(typeof(CompanyFilterType))]
        [UseSorting]
        public async Task<IEnumerable<Company>> Companies([Service] ILifetimeScope lifetimeScope, [Service] ICompanyRepository companyRepository, IResolverContext resolverContext, CancellationToken cancellationToken)
        {
            await using var scope = lifetimeScope.BeginLifetimeScope();
            var logger = scope.Resolve<ILogger<CompanyQuery>>();

            logger.LogDebug($"Starting {nameof(DocumentQuery)}.");

            var selections = (resolverContext.GetSelectedField()
                    .Selection
                    .SelectionSet
                    ?.Selections
                    .Select(x => ((FieldNode)x).Name.Value.ToLower()) ?? Enumerable.Empty<string>())
                .ToList();

            logger.LogDebug($"{nameof(CompanyQuery)}. Fields: {string.Join(", ", selections)}");

            var query = companyRepository.CompaniesQuery();

            if (selections.Contains(nameof(Company.OverridenByCompany).ToLower()))
            {
                query = query.Include(x => x.OverridenByCompany);
            }

            if (selections.Contains(nameof(Company.ParentCompanies).ToLower()))
            {
                query = query.Include(x => x.ParentCompanies).AsQueryable();
            }

            if (selections.Contains(nameof(Company.CompanyDocuments).ToLower()))
            {
                query = query.Include(x => x.CompanyDocuments).AsQueryable();
            }

            return query;
        }

        [UseOffsetPaging(IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering(typeof(CompanyFilterType))]
        [UseSorting]
        public async Task<IEnumerable<Company>> CompaniesPaging([Service] ILifetimeScope lifetimeScope, [Service] ICompanyRepository companyRepository, IResolverContext resolverContext, CancellationToken cancellationToken)
        {
            await using var scope = lifetimeScope.BeginLifetimeScope();
            var logger = scope.Resolve<ILogger<CompanyQuery>>();

            logger.LogDebug($"Starting {nameof(DocumentQuery)}.");

            var selections = (resolverContext.GetSelectedField()
                    .Selection
                    .SelectionSet
                    ?.Selections
                    .Select(x => ((FieldNode)x).Name.Value.ToLower()) ?? Enumerable.Empty<string>())
                .ToList();

            logger.LogDebug($"{nameof(CompanyQuery)}. Fields: {string.Join(", ", selections)}");

            var query = companyRepository.CompaniesQuery();

            if (selections.Contains(nameof(Company.OverridenByCompany).ToLower()))
            {
                query = query.Include(x => x.OverridenByCompany);
            }

            if (selections.Contains(nameof(Company.ParentCompanies).ToLower()))
            {
                query = query.Include(x => x.ParentCompanies).AsQueryable();
            }

            if (selections.Contains(nameof(Company.CompanyDocuments).ToLower()))
            {
                query = query.Include(x => x.CompanyDocuments).AsQueryable();
            }

            return query;
        }

        [UseProjection]
        [UseFiltering(typeof(ParentCompanyFilterType))]
        [UseSorting]
        public IQueryable<ParentCompany> ParentCompanies([Service] ICompanyRepository companyRepository)
        {
            return companyRepository.ParentCompaniesQuery().AsAsyncQueryable();
        }

        [UseProjection]
        [UseFiltering(typeof(CompanyDocumentFilterType))]
        [UseSorting]
        public async Task<IEnumerable<CompanyDocument>> CompanyDocuments([Service] ILifetimeScope lifetimeScope, IResolverContext resolverContext, CancellationToken cancellationToken)
        {
            await using var scope = lifetimeScope.BeginLifetimeScope();
            var companyRepository = scope.Resolve<ICompanyRepository>();

            var selections = (resolverContext.GetSelectedField()
                    .Selection
                    .SelectionSet
                    ?.Selections
                    .Select(x => ((FieldNode)x).Name.Value.ToLower()) ?? Enumerable.Empty<string>())
                .ToList();

            var query = companyRepository.CompanyDocumentsQuery().Filter(resolverContext);

            if (selections.Contains(nameof(CompanyDocument.Company).ToLower()))
            {
                query = query.Include(x => x.Company).AsQueryable();
            }

            var results = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

            return results;
        }
    }
}