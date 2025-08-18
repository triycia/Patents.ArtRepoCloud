using Patents.ArtRepoCloud.Domain.Aggregates.DocumentAggregate;
using Patents.ArtRepoCloud.Domain.Interfaces;
using HotChocolate.Resolvers;
using Patents.ArtRepoCloud.Domain.Extensions;
using MediatR;
using Autofac;
using Patents.ArtRepoCloud.GraphService.Application.Queries.DocumentStatus;
using Patents.ArtRepoCloud.GraphService.Extensions;
using Vikcher.Framework.Data.Cosmos;
using Patents.ArtRepoCloud.GraphService.Code.GraphQL.GraphQLTypes.ArtRepoDocumentTypes;
using HotChocolate.Data.Projections.Context;
using HotChocolate.Language;

namespace Patents.ArtRepoCloud.GraphService.GraphOperations
{
    [ExtendObjectType("Query")]
    public class DocumentQuery
    {
        private static readonly string[] DocumentDataFields = 
        {
            nameof(ArtRepoDocument.DocumentData),
            nameof(DocumentData.Authors),
            nameof(DocumentData.AssigneeName),
            nameof(DocumentData.TerminalDisclaimer),
            nameof(DocumentData.DocumentClassifications),
            nameof(DocumentData.DocumentTranslations),
            nameof(DocumentData.DefaultTranslation),
            nameof(DocumentData.UserNotes),
            nameof(DocumentData.UserUrl)
        };

        [UseProjection]
        [UseFiltering(typeof(ArtRepoDocumentFilterType))]
        [UseSorting]
        public async Task<IEnumerable<ArtRepoDocument>> Documents([Service] ILifetimeScope lifetimeScope, IResolverContext resolverContext, CancellationToken cancellationToken)
        {
            await using var scope = lifetimeScope.BeginLifetimeScope();
            var documentRepository = scope.Resolve<IDocumentRepository>();

            var selections = resolverContext.GetSelections().ToList();

            var documents = await documentRepository.QueryDocuments()
                .Filter(resolverContext)
                .ToListAsync(cancellationToken);

            documents = documents.ToList();

            if (selections.Contains(nameof(ArtRepoDocument.RepresentativeImage).ToLower()))
            {
                foreach (var d in documents)
                {
                    d.InitRepresentativeImage();
                }
            }

            if (DocumentDataFields.Any(f => selections.Contains(f.ToLower())))
            {
                var fileRepository = scope.Resolve<IFileRepository>();

                foreach (var d in documents.Where(d => !string.IsNullOrEmpty(d.DocumentDataBlobPath)))
                {
                    var path = fileRepository.BibDataPath(d.DocumentDataBlobPath!);

                    var documentDataStream = await fileRepository.GetAsync(path, cancellationToken).ConfigureAwait(false);

                    d.SetDocumentData(documentDataStream.ReadAs<DocumentData>());
                }
            }

            return documents;
        }

        [UseOffsetPaging(IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering(typeof(ArtRepoDocumentFilterType))]
        [UseSorting]
        public async Task<IEnumerable<ArtRepoDocument>> DocumentsPaging([Service] ILifetimeScope lifetimeScope, IResolverContext resolverContext, CancellationToken cancellationToken)
        {
            await using var scope = lifetimeScope.BeginLifetimeScope();
            var documentRepository = scope.Resolve<IDocumentRepository>();

            var selectionsSet = resolverContext
                .GetSelectedField()?
                .Selection?
                .SelectionSet?.Selections.FirstOrDefault() as HotChocolate.Language.FieldNode;

            var selections = selectionsSet?
                                 .SelectionSet?
                                 .Selections
                                 .Select(x => ((FieldNode)x).Name.Value.ToLower())
                             ?? Enumerable.Empty<string>();

            selections = selections.ToList();

            var documents = await documentRepository.QueryDocuments()
                .Filter(resolverContext)
                .ToListAsync(cancellationToken);

            documents = documents.ToList();

            if (selections.Contains(nameof(ArtRepoDocument.RepresentativeImage).ToLower()))
            {
                foreach (var d in documents)
                {
                    d.InitRepresentativeImage();
                }
            }

            if (DocumentDataFields.Any(f => selections.Contains(f.ToLower())))
            {
                var fileRepository = scope.Resolve<IFileRepository>();

                foreach (var d in documents.Where(d => !string.IsNullOrEmpty(d.DocumentDataBlobPath)))
                {
                    var path = fileRepository.BibDataPath(d.DocumentDataBlobPath!);

                    var documentDataStream = await fileRepository.GetAsync(path, cancellationToken).ConfigureAwait(false);

                    d.SetDocumentData(documentDataStream.ReadAs<DocumentData>());
                }
            }

            return documents;
        }

        [GraphQLName("documentStatus")]
        public async Task<DocumentStatusQueryResult> Status([Service] IMediator mediator, string referenceNumber, CancellationToken cancellationToken)
        {
            return await mediator.Send(new DocumentStatusQuery(referenceNumber), cancellationToken).ConfigureAwait(false);
        }
    }
} 