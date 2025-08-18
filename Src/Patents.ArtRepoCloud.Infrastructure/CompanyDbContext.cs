using Vikcher.Framework.Data.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Patents.ArtRepoCloud.Domain.Aggregates.CompanyAggregate;
using MediatR;

namespace Patents.ArtRepoCloud.Infrastructure
{
    public class CompanyDbContext : AbstractDbContext, IDbContext
    {
        public virtual DbSet<Company> Companies { get; protected set; }
        public virtual DbSet<ParentCompany> ParentCompanies { get; protected set; }
        public virtual DbSet<CompanyDocument> CompanyDocuments { get; protected set; }

        public CompanyDbContext(DbContextOptions<CompanyDbContext> options, IMediator mediator)
            : base(options, mediator)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            MapEntities(modelBuilder);
        }

        private void MapEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("Companies", "dbo");

                entity.HasKey(k => k.Id);

                entity
                    .Property(p => p.Id)
                    .HasColumnName("CompanyID")
                    .UseIdentityColumn();

                entity
                    .Property(p => p.CompanyName)
                    .IsRequired(false)
                    .IsUnicode()
                    .HasMaxLength(4000);

                entity
                    .Property(p => p.Status)
                    .IsRequired()
                    .HasColumnName("StatusID");

                entity
                    .Property(p => p.OverridenByCompanyId)
                    .HasColumnName("OverridenByCompanyID")
                    .IsRequired(false);

                entity.HasOne(d => d.OverridenByCompany)
                    .WithOne()
                    .HasForeignKey<Company>(d => d.OverridenByCompanyId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(d => d.ParentCompanies)
                    .WithOne()
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(d => d.CompanyDocuments)
                    .WithOne(d => d.Company)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<ParentCompany>(entity =>
            {
                entity.ToTable("ParentCompanies", "dbo");

                entity.HasKey(p => new { p.CompanyId, p.ParentCompanyId });

                entity
                    .Property(p => p.ParentCompanyId)
                    .IsRequired()
                    .HasColumnName("ParentCompanyID");

                entity
                    .Property(p => p.CompanyId)
                    .IsRequired()
                    .HasColumnName("CompanyID");

                entity.HasOne(d => d.Parent);
            });

            modelBuilder.Entity<CompanyDocument>(entity =>
            {
                entity.ToTable("CompanyDocuments", "dbo");

                entity.HasKey(p => new { p.CompanyId, p.ReferenceNumber });

                entity
                    .Property(p => p.CompanyId)
                    .IsRequired()
                    .HasColumnName("CompanyID");

                entity
                    .Property(p => p.ReferenceNumber)
                    .IsRequired()
                    .HasMaxLength(50);
            });
        }
    }
}