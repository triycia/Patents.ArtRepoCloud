namespace Patents.ArtRepoCloud.GraphService.Application.Commands.DeleteParentCompany
{
    public class DeleteParentCompanyCommandResult
    {

        public DeleteParentCompanyCommandResult(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public bool IsSuccess { get; }
    }
}