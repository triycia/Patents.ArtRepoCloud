namespace Patents.ArtRepoCloud.GraphService.Application.Commands.SetParentCompany
{
    public class SetParentCompanyCommandResult
    {
        public SetParentCompanyCommandResult(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public bool IsSuccess { get; }
    }
}