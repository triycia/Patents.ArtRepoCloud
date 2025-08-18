using Patents.ArtRepoCloud.GraphService.Application.Commands.AddOrUpdateCompany;
using MediatR;

namespace Patents.ArtRepoCloud.GraphService.GraphOperations
{
    [ExtendObjectType("Mutation")]
    public class CompanyMutation
    {
        public async Task<AddOrUpdateCompanyCommandResult> AddOrUpdateCompany([Service] IMediator mediator, AddOrUpdateCompanyCommand command, CancellationToken cancellationToken)
        {
            return await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<AddOrUpdateCompanyCommandResult> DeleteCompany([Service] IMediator mediator, AddOrUpdateCompanyCommand command, CancellationToken cancellationToken)
        {
            return await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        }
    }
}
