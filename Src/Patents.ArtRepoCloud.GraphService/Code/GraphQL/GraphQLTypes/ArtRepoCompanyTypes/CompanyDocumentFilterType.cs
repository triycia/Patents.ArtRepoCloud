using Patents.ArtRepoCloud.Domain.Aggregates.CompanyAggregate;
using HotChocolate.Data.Filters;

namespace Patents.ArtRepoCloud.GraphService.Code.GraphQL.GraphQLTypes.ArtRepoCompanyTypes
{
    public class CompanyDocumentFilterType : FilterInputType<CompanyDocument>
    {
        protected override void Configure(IFilterInputTypeDescriptor<CompanyDocument> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.AllowOr().Field(f => f.CompanyId).Type<CompanyIntOperationFilterInputType>();
            descriptor.AllowOr().Field(f => f.ReferenceNumber).Type<CompanyStringOperationFilterInputType>();
        }
    }
}