using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FakeSurveyGenerator.Infrastructure.Persistence.EntityConfigurations
{
    internal sealed class SurveyOptionEntityTypeConfiguration : IEntityTypeConfiguration<SurveyOption>
    {
        public void Configure(EntityTypeBuilder<SurveyOption> builder)
        {
            const string tableName = "SurveyOption";
            var sequenceName = $"{tableName}Seq";

            builder
                .ToTable(tableName, SurveyContext.DefaultSchema);

            builder
                .HasKey(o => o.Id);

            builder
                .Property(o => o.Id)
                .UseHiLo(sequenceName, SurveyContext.DefaultSchema);

            builder
                .Ignore(b => b.DomainEvents);

            builder
                .Property(o => o.OptionText)
                .HasMaxLength(250)
                .IsRequired()
                .HasConversion(domainValue => domainValue.Value, databaseValue => NonEmptyString.Create(databaseValue));

            builder
                .Property(o => o.NumberOfVotes)
                .IsRequired();

            builder
                .Property(o => o.PreferredNumberOfVotes);
        }
    }
}