using HotChocolate.Data.Filters;

namespace Patents.ArtRepoCloud.GraphService.Code.GraphQL.GraphQLTypes.ArtRepoDocumentTypes
{
    public class ArtRepoDocumentFilterType : FilterInputType<Domain.Aggregates.DocumentAggregate.ArtRepoDocument>
    {
        protected override void Configure(IFilterInputTypeDescriptor<Domain.Aggregates.DocumentAggregate.ArtRepoDocument> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.AllowOr().Field(f => f.ReferenceNumber).Type<DocumentStringOperationFilterInputType>();
            descriptor.AllowOr().Field(f => f.Country).Type<DocumentStringOperationFilterInputType>();
        }
    }
}