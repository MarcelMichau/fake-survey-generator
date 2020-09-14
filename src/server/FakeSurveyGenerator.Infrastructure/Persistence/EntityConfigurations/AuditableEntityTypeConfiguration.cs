using FakeSurveyGenerator.Shared.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FakeSurveyGenerator.Infrastructure.Persistence.EntityConfigurations
{
    internal class AuditableEntityTypeConfiguration<T> : IEntityTypeConfiguration<T> where T: AuditableEntity
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.Property(s => s.CreatedBy)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(s => s.CreatedOn)
                .IsRequired();

            builder.Property(s => s.ModifiedBy)
                .HasMaxLength(250);

            builder.Property(s => s.ModifiedOn);
        }
    }
}
