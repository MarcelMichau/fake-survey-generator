using FakeSurveyGenerator.Domain.AggregatesModel.UserAggregate;
using FakeSurveyGenerator.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FakeSurveyGenerator.Infrastructure.Persistence.EntityConfigurations
{
    internal sealed class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            const string tableName = "User";
            var sequenceName = $"{tableName}Seq";

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
                .HasMaxLength(250)
                .IsRequired()
                .HasConversion(domainValue => domainValue.Value, databaseValue => NonEmptyString.Create(databaseValue));

            builder
                .Property(u => u.EmailAddress)
                .HasMaxLength(250)
                .IsRequired()
                .HasConversion(domainValue => domainValue.Value, databaseValue => NonEmptyString.Create(databaseValue));

            builder
                .Property(u => u.ExternalUserId)
                .HasMaxLength(250)
                .IsRequired()
                .HasConversion(domainValue => domainValue.Value, databaseValue => NonEmptyString.Create(databaseValue));

            builder
                .HasMany(u => u.OwnedSurveys)
                .WithOne(s => s.Owner)
                .IsRequired();
        }
    }
}