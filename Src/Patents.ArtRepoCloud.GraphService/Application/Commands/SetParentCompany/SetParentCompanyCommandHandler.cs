using Patents.ArtRepoCloud.Domain.Aggregates.CompanyAggregate;
using Patents.ArtRepoCloud.Domain.Interfaces;
using Vikcher.Framework.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.SetParentCompany
{
    public class SetParentCompanyCommandHandler : IRequestHandler<SetParentCompanyCommand, SetParentCompanyCommandResult>
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<SetParentCompanyCommandHandler> _logger;

        public SetParentCompanyCommandHandler(ICompanyRepository companyRepository, ILogger<SetParentCompanyCommandHandler> logger)
        {
            _companyRepository = companyRepository;
            _logger = logger;
        }

        public async Task<SetParentCompanyCommandResult> Handle(SetParentCompanyCommand command, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting: {nameof(SetParentCompanyCommand)} for: {command.ToJson()}.");

            var parentCompany = await _companyRepository.ParentCompaniesQuery()
                .SingleOrDefaultAsync(x => x.CompanyId == command.CompanyId && x.ParentCompanyId == command.ParentCompanyId, cancellationToken)
                .ConfigureAwait(false);

            if (parentCompany == null)
            {
                var company = await _companyRepository.CompaniesQuery()
                    .SingleOrDefaultAsync(x => x.Id == command.CompanyId, cancellationToken)
                    .ConfigureAwait(false);

                if (company == null)
                {
                    throw new InvalidOperationException($"No Company found for {nameof(command.CompanyId)}:  {command.CompanyId}.");
                }

                var parent = await _companyRepository.CompaniesQuery()
                    .SingleOrDefaultAsync(x => x.Id == command.ParentCompanyId, cancellationToken)
                    .ConfigureAwait(false);

                if (parent == null)
                {
                    throw new InvalidOperationException($"No Company found for {nameof(command.ParentCompanyId)}: {command.ParentCompanyId}.");
                }

                await _companyRepository
                    .AddParentCompanyAsync(new ParentCompany(company.Id, parent.Id), cancellationToken)
                    .ConfigureAwait(false);

                await _companyRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            }

            return new SetParentCompanyCommandResult(true);
        }
    }
}