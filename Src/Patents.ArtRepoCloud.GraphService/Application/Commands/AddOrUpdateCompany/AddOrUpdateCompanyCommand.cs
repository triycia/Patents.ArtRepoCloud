using Patents.ArtRepoCloud.Domain.Aggregates.CompanyAggregate.Enums;
using MediatR;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.AddOrUpdateCompany
{
    public class AddOrUpdateCompanyCommand : IRequest<AddOrUpdateCompanyCommandResult>
    {
        public AddOrUpdateCompanyCommand(
            int? companyId,
            string companyName,
            CompanyReviewStatus status,
            int? overridenByCompanyId)
        {
            CompanyId = companyId;
            CompanyName = companyName;
            Status = status;
            OverridenByCompanyId = overridenByCompanyId;
        }

        public int? CompanyId { get; }
        public string CompanyName { get; }
        public CompanyReviewStatus Status { get; }
        public int? OverridenByCompanyId { get; }
    }
}
