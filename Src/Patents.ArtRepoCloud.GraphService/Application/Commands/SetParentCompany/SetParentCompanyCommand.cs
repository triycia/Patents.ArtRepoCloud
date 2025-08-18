using MediatR;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.SetParentCompany
{
    public class SetParentCompanyCommand : IRequest<SetParentCompanyCommandResult>
    {
        public SetParentCompanyCommand(int companyId, int parentCompanyId)
        {
            CompanyId = companyId;
            ParentCompanyId = parentCompanyId;
        }

        public int CompanyId { get; }

        public int ParentCompanyId { get; }
    }
}