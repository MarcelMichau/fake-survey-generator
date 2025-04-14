using FakeSurveyGenerator.Application.Domain.Surveys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FakeSurveyGenerator.Application.Infrastructure.Persistence.EntityConfigurations;

internal sealed class SurveyEntityTypeConfiguration : AuditableEntityTypeConfiguration<Survey>
{
    public override void Configure(EntityTypeBuilder<Survey> builder)
    {
        builder.ToTable("Survey");

        builder
            .HasKey(s => s.Id);

        builder
            .Property(s => s.Id)
            .UseHiLo("SurveySeq");

        builder
            .Ignore(s => s.DomainEvents);

        builder
            .Property(s => s.Topic)
            .IsRequired();

        builder
            .Property(s => s.RespondentType)
            .IsRequired();

        builder
            .Property(s => s.NumberOfRespondents)
            .IsRequired();

        builder
            .OwnsMany(s => s.Options, ownedNavigationBuilder =>
            {
                ownedNavigationBuilder.ToTable("SurveyOption");

                ownedNavigationBuilder
                    .Property(o => o.OptionText)
                    .IsRequired();

                ownedNavigationBuilder
                    .Property(o => o.NumberOfVotes)
                    .IsRequired();

                ownedNavigationBuilder
                    .Property(o => o.PreferredNumberOfVotes);
            });

        builder.Navigation(s => s.Options)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        base.Configure(builder);
    }
}