using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FakeSurveyGenerator.Infrastructure.Persistence.EntityConfigurations
{
    internal sealed class SurveyEntityTypeConfiguration : IEntityTypeConfiguration<Survey>
    {
        public void Configure(EntityTypeBuilder<Survey> surveyConfiguration)
        {
            const string tableName = "Survey";
            var sequenceName = $"{tableName}Seq";
            var foreignKeyName = $"{tableName}Id";

            surveyConfiguration
                .ToTable(tableName, SurveyContext.DefaultSchema);

            surveyConfiguration
                .HasKey(s => s.Id);

            surveyConfiguration
                .Property(s => s.Id)
                .UseHiLo(sequenceName, SurveyContext.DefaultSchema);

            surveyConfiguration
                .Ignore(s => s.DomainEvents);

            surveyConfiguration
                .Property(s => s.Topic)
                .HasMaxLength(250)
                .IsRequired()
                .HasConversion(domainValue => domainValue.Value, databaseValue => NonEmptyString.Create(databaseValue));

            surveyConfiguration
                .Property(s => s.RespondentType)
                .HasMaxLength(250)
                .IsRequired()
                .HasConversion(domainValue => domainValue.Value, databaseValue => NonEmptyString.Create(databaseValue));

            surveyConfiguration
                .Property(s => s.NumberOfRespondents)
                .IsRequired();

            surveyConfiguration
                .Property(s => s.CreatedOn)
                .IsRequired();

            surveyConfiguration
                .HasMany(s => s.Options)
                .WithOne()
                .HasForeignKey(foreignKeyName)
                .IsRequired();

            var navigation = surveyConfiguration.Metadata.FindNavigation(nameof(Survey.Options));
            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}