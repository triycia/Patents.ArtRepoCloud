using Patents.ArtRepoCloud.Domain.Aggregates.CompanyAggregate;
using HotChocolate.Data.Filters;

namespace Patents.ArtRepoCloud.GraphService.Code.GraphQL.GraphQLTypes.ArtRepoCompanyTypes
{
    public class CompanyFilterType : FilterInputType<Company>
    {
        protected override void Configure(IFilterInputTypeDescriptor<Company> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.AllowOr().Field(f => f.Id).Type<CompanyIntOperationFilterInputType>();
            descriptor.AllowOr().Field(f => f.OverridenByCompanyId).Type<CompanyIntOperationFilterInputType>();
            descriptor.AllowOr().Field(f => f.CompanyName).Type<CompanyStringOperationFilterInputType>();
            descriptor.AllowOr().Field(f => f.Status).Type<CompanyStringOperationFilterInputType>();
        }
    }
}