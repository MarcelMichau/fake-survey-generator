using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FakeSurveyGenerator.Infrastructure.EntityConfigurations
{
    public class SurveyOptionEntityTypeConfiguration : IEntityTypeConfiguration<SurveyOption>
    {
        public void Configure(EntityTypeBuilder<SurveyOption> surveyOptionConfiguration)
        {
            surveyOptionConfiguration.ToTable("SurveyOption", SurveyContext.DefaultSchema);

            surveyOptionConfiguration.HasKey(o => o.Id);

            surveyOptionConfiguration.Property(o => o.Id)
                .UseHiLo("SurveyOptionSeq", SurveyContext.DefaultSchema);

            surveyOptionConfiguration.Ignore(b => b.DomainEvents);

            surveyOptionConfiguration.Property<string>("OptionText")
                .HasMaxLength(250)
                .IsRequired();

            surveyOptionConfiguration.Property<int>("NumberOfVotes").IsRequired();
            surveyOptionConfiguration.Property<int>("PreferredNumberOfVotes");
        }
    }
}
