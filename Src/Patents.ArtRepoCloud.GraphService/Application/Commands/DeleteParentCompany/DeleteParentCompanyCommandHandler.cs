using Patents.ArtRepoCloud.Domain.Interfaces;
using Vikcher.Framework.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.DeleteParentCompany
{
    public class DeleteParentCompanyCommandHandler : IRequestHandler<DeleteParentCompanyCommand, DeleteParentCompanyCommandResult>
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<DeleteParentCompanyCommandHandler> _logger;

        public DeleteParentCompanyCommandHandler(ICompanyRepository companyRepository, ILogger<DeleteParentCompanyCommandHandler> logger)
        {
            _companyRepository = companyRepository;
            _logger = logger;
        }

        public async Task<DeleteParentCompanyCommandResult> Handle(DeleteParentCompanyCommand command, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting: {nameof(DeleteParentCompanyCommand)} for: {command.ToJson()}.");

            var parentCompany = await _companyRepository.ParentCompaniesQuery()
                .SingleOrDefaultAsync(x => x.CompanyId == command.CompanyId && x.ParentCompanyId == command.ParentCompanyId, cancellationToken)
                .ConfigureAwait(false);

            if (parentCompany == null)
            {
                throw new InvalidOperationException($"Company with Id: {command.CompanyId} not found.");
            }

            _companyRepository.DeleteParentCompany(parentCompany);

            await _companyRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new DeleteParentCompanyCommandResult(true);
        }
    }
}