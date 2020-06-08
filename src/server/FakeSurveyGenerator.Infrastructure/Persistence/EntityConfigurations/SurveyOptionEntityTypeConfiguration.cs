using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FakeSurveyGenerator.Infrastructure.Persistence.EntityConfigurations
{
    internal sealed class SurveyOptionEntityTypeConfiguration : IEntityTypeConfiguration<SurveyOption>
    {
        public void Configure(EntityTypeBuilder<SurveyOption> surveyOptionConfiguration)
        {
            const string tableName = "SurveyOption";
            var sequenceName = $"{tableName}Seq";

            surveyOptionConfiguration
                .ToTable(tableName, SurveyContext.DefaultSchema);

            surveyOptionConfiguration
                .HasKey(o => o.Id);

            surveyOptionConfiguration
                .Property(o => o.Id)
                .UseHiLo(sequenceName, SurveyContext.DefaultSchema);

            surveyOptionConfiguration
                .Ignore(b => b.DomainEvents);

            surveyOptionConfiguration
                .Property(o => o.OptionText)
                .HasMaxLength(250)
                .IsRequired()
                .HasConversion(domainValue => domainValue.Value, databaseValue => NonEmptyString.Create(databaseValue));;

            surveyOptionConfiguration
                .Property(o => o.NumberOfVotes)
                .IsRequired();

            surveyOptionConfiguration
                .Property(o => o.PreferredNumberOfVotes);
        }
    }
}
