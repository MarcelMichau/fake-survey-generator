﻿using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FakeSurveyGenerator.Infrastructure.Persistence.EntityConfigurations;

internal sealed class SurveyEntityTypeConfiguration : AuditableEntityTypeConfiguration<Survey>
{
    public override void Configure(EntityTypeBuilder<Survey> builder)
    {
        const string tableName = "Survey";
        const string sequenceName = $"{tableName}Seq";

        builder
            .ToTable(tableName, SurveyContext.DefaultSchema);

        builder
            .HasKey(s => s.Id);

        builder
            .Property(s => s.Id)
            .UseHiLo(sequenceName, SurveyContext.DefaultSchema);

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
                const string surveyOptionTableName = "SurveyOption";

                ownedNavigationBuilder
                    .ToTable(surveyOptionTableName, SurveyContext.DefaultSchema);

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