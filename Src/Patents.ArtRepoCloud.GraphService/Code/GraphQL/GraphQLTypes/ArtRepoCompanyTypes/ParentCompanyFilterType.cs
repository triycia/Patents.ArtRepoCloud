using Patents.ArtRepoCloud.Domain.Aggregates.CompanyAggregate;
using HotChocolate.Data.Filters;

namespace Patents.ArtRepoCloud.GraphService.Code.GraphQL.GraphQLTypes.ArtRepoCompanyTypes
{
    public class ParentCompanyFilterType : FilterInputType<ParentCompany>
    {
        protected override void Configure(IFilterInputTypeDescriptor<ParentCompany> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.AllowOr().Field(f => f.CompanyId).Type<CompanyIntOperationFilterInputType>();
            descriptor.AllowOr().Field(f => f.ParentCompanyId).Type<CompanyIntOperationFilterInputType>();
        }
    }
}
