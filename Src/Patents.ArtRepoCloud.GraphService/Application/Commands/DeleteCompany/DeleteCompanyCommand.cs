using MediatR;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.DeleteCompany
{
    public class DeleteCompanyCommand : IRequest<DeleteCompanyCommandResult>
    {
        public DeleteCompanyCommand(int companyId)
        {
            CompanyId = companyId;
        }

        public int CompanyId { get; }
    }
}