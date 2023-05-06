using System.Reflection;
using FakeSurveyGenerator.Application.Common.DomainEvents;
using FakeSurveyGenerator.Application.Common.Persistence;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.AggregatesModel.UserAggregate;
using FakeSurveyGenerator.Domain.Common;
using FakeSurveyGenerator.Infrastructure.Persistence.Interceptors;
using FakeSurveyGenerator.Shared.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Infrastructure.Persistence;

public sealed class SurveyContext : DbContext, ISurveyContext
{
    private readonly IDomainEventService _domainEventService = null!;
    private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor = null!;
    private readonly ILogger _logger;

    public const string DefaultSchema = "Survey";

    public DbSet<User> Users => Set<User>();
    public DbSet<Survey> Surveys => Set<Survey>();
    public DbSet<SurveyOption> SurveyOptions => Set<SurveyOption>();

    public SurveyContext(DbContextOptions options, ILogger<SurveyContext> logger) : base(options)
    {
        if (options is null)
            throw new ArgumentNullException(nameof(options));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public SurveyContext(DbContextOptions options, IDomainEventService domainEventService, AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor, ILogger<SurveyContext> logger) : base(options)
    {
        if (options is null)
            throw new ArgumentNullException(nameof(options));

        _domainEventService = domainEventService ?? throw new ArgumentNullException(nameof(domainEventService));
        _auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditableEntitySaveChangesInterceptor);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<NonEmptyString>()
            .HaveMaxLength(250)
            .HaveConversion<NonEmptyStringValueConverter>();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await DispatchEvents(cancellationToken);

            var recordsAffected = await base.SaveChangesAsync(cancellationToken);

            return recordsAffected;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred during a database update");
            return 0;
        }
    }

    private async Task DispatchEvents(CancellationToken cancellationToken)
    {
        var entities = ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ToList().ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await _domainEventService.Publish(domainEvent, cancellationToken);
        }
    }
}