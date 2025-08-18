using HotChocolate.Data.Filters;

namespace Patents.ArtRepoCloud.GraphService.Code.GraphQL.GraphQLTypes.ArtRepoCompanyTypes
{
    public class CompanyIntOperationFilterInputType : IntOperationFilterInputType
    {
        protected override void Configure(IFilterInputTypeDescriptor descriptor)
        {
            descriptor.Operation(DefaultFilterOperations.Equals).Type<IntType>();
            descriptor.Operation(DefaultFilterOperations.NotEquals).Type<IntType>();
            descriptor.Operation(DefaultFilterOperations.In).Type<ListType<IntType>>();
            descriptor.Operation(DefaultFilterOperations.NotIn).Type<ListType<IntType>>();

            descriptor.Extend();
        }
    }
}