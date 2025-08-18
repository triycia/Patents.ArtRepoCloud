namespace Patents.ArtRepoCloud.GraphService.Application.Commands.AddOrUpdateCompany
{
    public class AddOrUpdateCompanyCommandResult
    {
        public AddOrUpdateCompanyCommandResult(int companyId)
        {
            CompanyId = companyId;
        }

        public int CompanyId { get; }
    }
}