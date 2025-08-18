using Patents.ArtRepoCloud.Domain.Aggregates.CompanyAggregate;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Vikcher.Framework.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.AddOrUpdateCompany
{
    public class AddOrUpdateCompanyCommandHandler : IRequestHandler<AddOrUpdateCompanyCommand, AddOrUpdateCompanyCommandResult>
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<AddOrUpdateCompanyCommandHandler> _logger;

        public AddOrUpdateCompanyCommandHandler(ICompanyRepository companyRepository, ILogger<AddOrUpdateCompanyCommandHandler> logger)
        {
            _companyRepository = companyRepository;
            _logger = logger;
        }

        public async Task<AddOrUpdateCompanyCommandResult> Handle(AddOrUpdateCompanyCommand command, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting: {nameof(AddOrUpdateCompanyCommand)} for: {command.ToJson()}.");

            Company? company;

            if (command.CompanyId == null)
            {
                company = new Company(command.CompanyName, command.Status, command.OverridenByCompanyId);

                await _companyRepository.AddCompanyAsync(company, cancellationToken).ConfigureAwait(false);

                await _companyRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation(
                    $"Created new Company: Id/CompanyName {company.Id}/{company.CompanyName}.");
            }
            else
            {
                company = await _companyRepository.CompaniesQuery()
                        .SingleOrDefaultAsync(x => x.Id == command.CompanyId, cancellationToken)
                        .ConfigureAwait(false);

                if (company == null)
                {
                    throw new InvalidOperationException($"Company with Id: {command.CompanyId} not found.");
                }

                if (company.CompanyName != command.CompanyName)
                {
                    company.SetCompanyName(command.CompanyName);
                }

                if (company.Status != command.Status)
                {
                    company.SetStatus(command.Status);
                }

                if (company.OverridenByCompanyId != command.OverridenByCompanyId)
                {
                    company.SetOverridenByCompanyId(command.OverridenByCompanyId);
                }

                _logger.LogInformation(
                    $"Updated Company: Id/CompanyName {company.Id}/{company.CompanyName}.");
            }

            return new AddOrUpdateCompanyCommandResult(company.Id);
        }
    }
}
