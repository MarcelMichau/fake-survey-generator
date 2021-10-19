using FakeSurveyGenerator.Domain.AggregatesModel.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FakeSurveyGenerator.Infrastructure.Persistence.EntityConfigurations
{
    internal sealed class UserEntityTypeConfiguration : AuditableEntityTypeConfiguration<User>
    {
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            const string tableName = "User";
            const string sequenceName = $"{tableName}Seq";

            builder
                .ToTable(tableName, SurveyContext.DefaultSchema);

            builder
                .HasKey(u => u.Id);

            builder
                .Property(u => u.Id)
                .UseHiLo(sequenceName, SurveyContext.DefaultSchema);

            builder
                .Ignore(u => u.DomainEvents);

            builder
                .Property(u => u.DisplayName)
                .IsRequired();

            builder
                .Property(u => u.EmailAddress)
                .IsRequired();

            builder
                .Property(u => u.ExternalUserId)
                .IsRequired();

            builder
                .HasIndex(u => u.ExternalUserId)
                .IsUnique();

            builder
                .HasMany(u => u.OwnedSurveys)
                .WithOne(s => s.Owner)
                .IsRequired();

            base.Configure(builder);
        }
    }
}