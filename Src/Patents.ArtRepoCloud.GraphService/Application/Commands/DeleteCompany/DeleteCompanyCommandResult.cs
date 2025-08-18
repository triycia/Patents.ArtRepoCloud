namespace Patents.ArtRepoCloud.GraphService.Application.Commands.DeleteCompany
{
    public class DeleteCompanyCommandResult
    {
        public DeleteCompanyCommandResult(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public bool IsSuccess { get; }
    }
}