using Patents.ArtRepoCloud.Domain.Aggregates.CompanyAggregate;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Patents.ArtRepoCloud.Infrastructure.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly CompanyDbContext _companyDbContext;

        public CompanyRepository(CompanyDbContext tenantDbContext)
        {
            _companyDbContext = tenantDbContext;
        }

        public IQueryable<Company> CompaniesQuery()
        {
            return _companyDbContext.Companies;
        }

        public async Task AddCompanyAsync(Company company, CancellationToken cancellationToken)
        {
            await _companyDbContext.Companies.AddAsync(company, cancellationToken);
        }

        public void DeleteCompany(Company company)
        {
            _companyDbContext.Companies.Remove(company);
        }

        public IQueryable<ParentCompany> ParentCompaniesQuery()
        {
            return _companyDbContext.ParentCompanies;
        }

        public async Task AddParentCompanyAsync(ParentCompany parentCompany, CancellationToken cancellationToken)
        {
            await _companyDbContext.ParentCompanies.AddAsync(parentCompany, cancellationToken);
        }

        public void DeleteParentCompany(ParentCompany parentCompany)
        {
            _companyDbContext.ParentCompanies.Remove(parentCompany);
        }

        public IQueryable<CompanyDocument> CompanyDocumentsQuery()
        {
            return _companyDbContext.CompanyDocuments;
        }

        public async Task AddCompanyDocumentAsync(CompanyDocument companyDocument, CancellationToken cancellationToken)
        {
            await _companyDbContext.CompanyDocuments.AddAsync(companyDocument, cancellationToken);
        }

        public void DeleteCompanyDocument(CompanyDocument companyDocument)
        {
            _companyDbContext.CompanyDocuments.Remove(companyDocument);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _companyDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}