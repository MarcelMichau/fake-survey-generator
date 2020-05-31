using System;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FakeSurveyGenerator.Infrastructure.EntityConfigurations
{
    public class SurveyEntityTypeConfiguration : IEntityTypeConfiguration<Survey>
    {
        public void Configure(EntityTypeBuilder<Survey> surveyConfiguration)
        {
            surveyConfiguration.ToTable("Survey", SurveyContext.DefaultSchema);

            surveyConfiguration.HasKey(s => s.Id);

            surveyConfiguration.Property(s => s.Id)
                .UseHiLo("SurveySeq", SurveyContext.DefaultSchema);

            surveyConfiguration.Ignore(s => s.DomainEvents);

            surveyConfiguration.Property<string>("Topic")
                .HasMaxLength(250)
                .IsRequired();

            surveyConfiguration.Property<string>("RespondentType")
                .HasMaxLength(250)
                .IsRequired();

            surveyConfiguration.Property<int>("NumberOfRespondents").IsRequired();
            surveyConfiguration.Property<DateTime>("CreatedOn").IsRequired();

            surveyConfiguration.HasMany(s => s.Options)
                .WithOne()
                .HasForeignKey("SurveyId")
                .IsRequired();

            var navigation = surveyConfiguration.Metadata.FindNavigation(nameof(Survey.Options));
            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
