using System.Reflection;
using FakeSurveyGenerator.Application.Domain.Shared;
using FakeSurveyGenerator.Application.Domain.Surveys;
using FakeSurveyGenerator.Application.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Application.Infrastructure.Persistence;

public sealed class SurveyContext : DbContext
{
    public SurveyContext(DbContextOptions options) : base(options)
    {
        ArgumentNullException.ThrowIfNull(options);
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Survey> Surveys => Set<Survey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.HasDefaultSchema("Survey");

        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<NonEmptyString>()
            .HaveMaxLength(250)
            .HaveConversion<NonEmptyStringValueConverter>();
    }
}