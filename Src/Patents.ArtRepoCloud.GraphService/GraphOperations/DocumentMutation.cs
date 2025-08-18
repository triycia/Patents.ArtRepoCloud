using Patents.ArtRepoCloud.GraphService.Extensions;
using Patents.ArtRepoCloud.GraphService.Application.Commands.CreateDocument;
using Patents.ArtRepoCloud.GraphService.Application.Commands.DeleteDocumentFile;
using Patents.ArtRepoCloud.GraphService.Application.Commands.DeleteImage;
using Patents.ArtRepoCloud.GraphService.Application.Commands.EnqueueBatchCommand;
using Patents.ArtRepoCloud.GraphService.Application.Commands.ImageExtraction;
using Patents.ArtRepoCloud.GraphService.Application.Commands.ParseReferenceNumbers;
using Patents.ArtRepoCloud.GraphService.Application.Commands.UpdateDocument;
using Patents.ArtRepoCloud.GraphService.Application.Commands.UploadDocumentFile;
using Patents.ArtRepoCloud.GraphService.Application.Commands.ZipPdf;
using Patents.ArtRepoCloud.GraphService.DataModels;
using MediatR;
using Patents.ArtRepoCloud.GraphService.Application.Commands.DocumentFileImageExtraction;

namespace Patents.ArtRepoCloud.GraphService.GraphOperations
{
    [ExtendObjectType("Mutation")]
    public class DocumentMutation 
    {
        private readonly IHttpContextAccessor _accessor;

        public DocumentMutation(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public async Task<ParseReferenceNumbersCommandResult> ParseReferenceNumbers([Service] IMediator mediator, ParseReferenceNumbersCommand command, CancellationToken cancellationToken)
        {
            return await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<UploadDocumentFileCommandResult> UploadDocumentFile([Service] IMediator mediator, FileUploadModel command, CancellationToken cancellationToken)
        {
            var stream = _accessor.Attachment();

            var result = await mediator.Send(
                new UploadDocumentFileCommand(
                    command.ReferenceNumber,
                    command.FileName,
                    command.DesignatedImages,
                    command.RepresentativeImagePageNumber,
                    stream), cancellationToken).ConfigureAwait(false);

            return result;
        }

        public async Task<DeleteDocumentFileCommandResult> DeleteDocumentFile([Service] IMediator mediator, DeleteDocumentFileCommand command, CancellationToken cancellationToken)
        {
            return await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<DeleteImageCommandResult> DeletedDocumentImage([Service] IMediator mediator, DeletedDocumentImageCommand command, CancellationToken cancellationToken)
        {
            return await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<EnqueueBatchCommandResult> EnqueueBatch([Service] IMediator mediator, EnqueueBatchCommand command, CancellationToken cancellationToken)
        {
            return await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<CreateDocumentCommandResult> CreateDocument([Service] IMediator mediator, CreateDocumentCommand command, CancellationToken cancellationToken)
        {
            return await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<UpdateDocumentCommandResult> UpdateDocument([Service] IMediator mediator, UpdateDocumentCommand command, CancellationToken cancellationToken)
        {
            return await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<DocumentFileImageExtractionCommandResult> DocumentFileImageExtraction([Service] IMediator mediator, DocumentFileImageExtractionCommand command, CancellationToken cancellationToken)
        {
            return await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<ImageExtractionCommandResult> ImageExtraction([Service] IMediator mediator, ImageExtractionCommand command, CancellationToken cancellationToken)
        {
            return await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<CreatePdfZipCommandResult> CreatePdfZip([Service] IMediator mediator, CreatePdfZipCommand command, CancellationToken cancellationToken)
        {
            return await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        }
    }
}