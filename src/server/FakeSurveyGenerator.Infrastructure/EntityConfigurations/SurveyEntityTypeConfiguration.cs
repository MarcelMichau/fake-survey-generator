using System;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FakeSurveyGenerator.Infrastructure.EntityConfigurations
{
    public class SurveyEntityTypeConfiguration : IEntityTypeConfiguration<Survey>
    {
        public void Configure(EntityTypeBuilder<Survey> surveyConfiguration)
        {
            surveyConfiguration.ToTable("Survey", SurveyContext.DefaultSchema);

            surveyConfiguration.HasKey(o => o.Id);

            surveyConfiguration.Property(o => o.Id)
                .UseHiLo("SurveySeq", SurveyContext.DefaultSchema);

            surveyConfiguration.Ignore(b => b.DomainEvents);

            surveyConfiguration.Property<string>("Topic")
                .HasMaxLength(250)
                .IsRequired();

            surveyConfiguration.Property<string>("RespondentType")
                .HasMaxLength(250)
                .IsRequired();

            surveyConfiguration.Property<int>("NumberOfRespondents").IsRequired();
            surveyConfiguration.Property<DateTime>("CreatedOn").IsRequired();

            var navigation = surveyConfiguration.Metadata.FindNavigation(nameof(Survey.Options));
            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
