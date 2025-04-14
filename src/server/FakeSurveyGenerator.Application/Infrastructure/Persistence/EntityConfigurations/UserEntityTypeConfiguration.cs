using FakeSurveyGenerator.Application.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FakeSurveyGenerator.Application.Infrastructure.Persistence.EntityConfigurations;

internal sealed class UserEntityTypeConfiguration : AuditableEntityTypeConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");

        builder
            .HasKey(u => u.Id);

        builder
            .Property(u => u.Id)
            .UseHiLo("UserSeq");

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