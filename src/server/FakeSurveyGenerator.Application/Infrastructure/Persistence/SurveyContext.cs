using System.Reflection;
using FakeSurveyGenerator.Application.Domain.Shared;
using FakeSurveyGenerator.Application.Domain.Shared.SeedWork;
using FakeSurveyGenerator.Application.Domain.Surveys;
using FakeSurveyGenerator.Application.Domain.Users;
using FakeSurveyGenerator.Application.Infrastructure.Persistence.Interceptors;
using FakeSurveyGenerator.Application.Shared.DomainEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Application.Infrastructure.Persistence;

public sealed class SurveyContext : DbContext
{
    public const string DefaultSchema = "Survey";
    private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor = null!;
    private readonly IDomainEventService _domainEventService = null!;
    private readonly ILogger _logger;

    public SurveyContext(DbContextOptions options, ILogger<SurveyContext> logger) : base(options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public SurveyContext(DbContextOptions options, IDomainEventService domainEventService,
        AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor,
        ILogger<SurveyContext> logger) : base(options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _domainEventService = domainEventService ?? throw new ArgumentNullException(nameof(domainEventService));
        _auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Survey> Surveys => Set<Survey>();

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

        foreach (var domainEvent in domainEvents) await _domainEventService.Publish(domainEvent, cancellationToken);
    }
}