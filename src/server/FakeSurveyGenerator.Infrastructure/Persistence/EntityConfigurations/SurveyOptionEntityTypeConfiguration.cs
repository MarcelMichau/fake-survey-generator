using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FakeSurveyGenerator.Infrastructure.Persistence.EntityConfigurations;

internal sealed class SurveyOptionEntityTypeConfiguration : AuditableEntityTypeConfiguration<SurveyOption>
{
    public override void Configure(EntityTypeBuilder<SurveyOption> builder)
    {
        const string tableName = "SurveyOption";
        const string sequenceName = $"{tableName}Seq";

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
            .IsRequired();

        builder
            .Property(o => o.NumberOfVotes)
            .IsRequired();

        builder
            .Property(o => o.PreferredNumberOfVotes);

        base.Configure(builder);
    }
}