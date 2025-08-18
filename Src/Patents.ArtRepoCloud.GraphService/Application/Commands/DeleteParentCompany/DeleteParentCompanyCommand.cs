using MediatR;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.DeleteParentCompany
{
    public class DeleteParentCompanyCommand : IRequest<DeleteParentCompanyCommandResult>
    {
        public DeleteParentCompanyCommand(int companyId, int parentCompanyId)
        {
            CompanyId = companyId;
            ParentCompanyId = parentCompanyId;
        }

        public int CompanyId { get; }

        public int ParentCompanyId { get; }
    }
}