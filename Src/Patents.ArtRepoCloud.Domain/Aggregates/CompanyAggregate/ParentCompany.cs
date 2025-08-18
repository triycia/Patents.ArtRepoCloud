using System.ComponentModel.DataAnnotations.Schema;

namespace Patents.ArtRepoCloud.Domain.Aggregates.CompanyAggregate
{
    public class ParentCompany
    {
        public ParentCompany() { }

        public ParentCompany(int companyId, int parentCompanyId)
        {
            CompanyId = companyId;
            ParentCompanyId = parentCompanyId;
        }

        [ForeignKey("Company")]
        public int CompanyId { get; private set; }

        [ForeignKey("Parent")]
        public int ParentCompanyId { get; private set; }

        [NotMapped] public virtual Company? Parent { get; private set; }
    }
}