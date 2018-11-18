using System;
using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FakeSurveyGenerator.Infrastructure
{
    public class SurveyContext : DbContext, IUnitOfWork
    {
        public SurveyContext(DbContextOptions options) : base(options)
        {   }

        const string DEFAULT_SCHEMA = "Survey";

        public DbSet<Survey> Surveys { get; set; }
        public DbSet<SurveyOption> SurveyOptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Survey>(ConfigureSurvey);
            modelBuilder.Entity<SurveyOption>(ConfigureSurveyOption);
        }

        private void ConfigureSurvey(EntityTypeBuilder<Survey> surveyConfiguration)
        {
            surveyConfiguration.ToTable("Survey", DEFAULT_SCHEMA);

            surveyConfiguration.HasKey(o => o.Id);

            surveyConfiguration.Property(o => o.Id)
                .ForSqlServerUseSequenceHiLo("SurveySeq", DEFAULT_SCHEMA);

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

        private void ConfigureSurveyOption(EntityTypeBuilder<SurveyOption> surveyOptionConfiguration)
        {
            surveyOptionConfiguration.ToTable("SurveyOption", DEFAULT_SCHEMA);

            surveyOptionConfiguration.HasKey(o => o.Id);

            surveyOptionConfiguration.Property(o => o.Id)
                .ForSqlServerUseSequenceHiLo("SurveyOptionSeq", DEFAULT_SCHEMA);

            surveyOptionConfiguration.Ignore(b => b.DomainEvents);

            surveyOptionConfiguration.Property<string>("OptionText")
                .HasMaxLength(250)
                .IsRequired();

            surveyOptionConfiguration.Property<int>("NumberOfVotes").IsRequired();
            surveyOptionConfiguration.Property<int>("PreferredOutcomeRank");
        }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
