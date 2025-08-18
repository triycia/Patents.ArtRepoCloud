namespace Patents.ArtRepoCloud.GraphService.Code.GraphQL.GraphQLTypes.ArtRepoDocumentTypes
{
    public class ArtRepoDocumentType : ObjectType<Domain.Aggregates.DocumentAggregate.ArtRepoDocument>
    {
        protected override void Configure(IObjectTypeDescriptor<Domain.Aggregates.DocumentAggregate.ArtRepoDocument> descriptor)
        {
            descriptor.Field(x => x.DomainEvents).Ignore();
            descriptor.Field(x => x.PartitionKey).Ignore();
            descriptor.Field(x => x.FamilyExtendeds).Ignore();
            descriptor.Field(x => x.DocumentDataBlobPath).Ignore();
            descriptor.Field(x => x.TimeToLive).Ignore();
        }
    }
}