using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FakeSurveyGenerator.Infrastructure.Persistence.EntityConfigurations
{
    internal sealed class SurveyEntityTypeConfiguration : AuditableEntityTypeConfiguration<Survey>
    {
        public override void Configure(EntityTypeBuilder<Survey> builder)
        {
            const string tableName = "Survey";
            var sequenceName = $"{tableName}Seq";
            var foreignKeyName = $"{tableName}Id";

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
                .HasMaxLength(250)
                .IsRequired()
                .HasConversion(DomainConversionProviders.NonEmptyStringToString, DomainConversionProviders.StringToNonEmptyString);

            builder
                .Property(s => s.RespondentType)
                .HasMaxLength(250)
                .IsRequired()
                .HasConversion(DomainConversionProviders.NonEmptyStringToString, DomainConversionProviders.StringToNonEmptyString);

            builder
                .Property(s => s.NumberOfRespondents)
                .IsRequired();

            builder
                .HasMany(s => s.Options)
                .WithOne()
                .HasForeignKey(foreignKeyName)
                .IsRequired();

            var navigation = builder.Metadata.FindNavigation(nameof(Survey.Options));
            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);

            base.Configure(builder);
        }
    }
}