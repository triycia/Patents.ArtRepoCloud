using Patents.ArtRepoCloud.Domain.Interfaces;
using Vikcher.Framework.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.DeleteCompany
{
    public class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand, DeleteCompanyCommandResult>
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<DeleteCompanyCommandHandler> _logger;

        public DeleteCompanyCommandHandler(ICompanyRepository companyRepository, ILogger<DeleteCompanyCommandHandler> logger)
        {
            _companyRepository = companyRepository;
            _logger = logger;
        }

        public async Task<DeleteCompanyCommandResult> Handle(DeleteCompanyCommand command, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting: {nameof(DeleteCompanyCommand)} for: {command.ToJson()}.");

            var company = await _companyRepository.CompaniesQuery()
                .SingleOrDefaultAsync(x => x.Id == command.CompanyId, cancellationToken)
                .ConfigureAwait(false);

            if (company == null)
            {
                throw new InvalidOperationException($"Company with Id: {command.CompanyId} not found.");
            }

            _companyRepository.DeleteCompany(company);

            await _companyRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new DeleteCompanyCommandResult(true);
        }
    }
}
