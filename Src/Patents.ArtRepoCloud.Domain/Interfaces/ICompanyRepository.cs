using Patents.ArtRepoCloud.Domain.Aggregates.CompanyAggregate;

namespace Patents.ArtRepoCloud.Domain.Interfaces
{
    public interface ICompanyRepository
    {
        IQueryable<Company> CompaniesQuery();
        Task AddCompanyAsync(Company company, CancellationToken cancellationToken);
        void DeleteCompany(Company company);
        IQueryable<ParentCompany> ParentCompaniesQuery();
        Task AddParentCompanyAsync(ParentCompany parentCompany, CancellationToken cancellationToken);
        void DeleteParentCompany(ParentCompany parentCompany);
        IQueryable<CompanyDocument> CompanyDocumentsQuery();
        Task AddCompanyDocumentAsync(CompanyDocument companyDocument, CancellationToken cancellationToken);
        void DeleteCompanyDocument(CompanyDocument companyDocument);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}