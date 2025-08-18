using Patents.ArtRepoCloud.Domain.Aggregates.CompanyAggregate.Enums;
using Vikcher.Framework.Data.EntityFramework;
using System.ComponentModel.DataAnnotations.Schema;

namespace Patents.ArtRepoCloud.Domain.Aggregates.CompanyAggregate
{
    public class Company : IAggregateRoot
    {
        public Company() { }

        public Company(int id, string? companyName, CompanyReviewStatus status, int? overridenByCompanyId)
        {
            Id = id;
            CompanyName = companyName;
            Status = status;
            OverridenByCompanyId = overridenByCompanyId;
        }

        public Company(string? companyName, CompanyReviewStatus status, int? overridenByCompanyId)
        {
            CompanyName = companyName;
            Status = status;
            OverridenByCompanyId = overridenByCompanyId;
        }

        public int Id { get; private set; }
        public string? CompanyName { get; private set; }
        public CompanyReviewStatus Status { get; private set; }
        public int? OverridenByCompanyId { get; private set; }

        [NotMapped] public virtual Company? OverridenByCompany { get; private set; }

        [NotMapped] public virtual IReadOnlyCollection<CompanyDocument>? CompanyDocuments { get; private set; } =
            new List<CompanyDocument>();
        [NotMapped] public virtual IReadOnlyCollection<ParentCompany> ParentCompanies { get; private set; }

        public void SetCompanyName(string name)
        {
            CompanyName = name;
        }

        public void SetStatus(CompanyReviewStatus status)
        {
            Status = status;
        }

        public void SetOverridenByCompanyId(int? overridenByCompanyId)
        {
            OverridenByCompanyId = overridenByCompanyId;
        }
    }
}