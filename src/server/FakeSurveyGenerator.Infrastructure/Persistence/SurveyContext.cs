using System.Reflection;
using FakeSurveyGenerator.Application.Common.DomainEvents;
using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Application.Common.Persistence;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.AggregatesModel.UserAggregate;
using FakeSurveyGenerator.Domain.Common;
using FakeSurveyGenerator.Shared.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Infrastructure.Persistence;

public sealed class SurveyContext : DbContext, ISurveyContext
{
    private readonly IUserService _userService;
    private readonly IDomainEventService _domainEventService;
    private readonly ILogger _logger;

    public const string DefaultSchema = "Survey";

    public DbSet<User> Users { get; set; }
    public DbSet<Survey> Surveys { get; set; }
    public DbSet<SurveyOption> SurveyOptions { get; set; }

    public SurveyContext(DbContextOptions options, IUserService userService, ILogger<SurveyContext> logger) : base(options)
    {
        if (options is null)
            throw new ArgumentNullException(nameof(options));

        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public SurveyContext(DbContextOptions options, IDomainEventService domainEventService, IUserService userService, ILogger<SurveyContext> logger) : base(options)
    {
        if (options is null)
            throw new ArgumentNullException(nameof(options));

        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _domainEventService = domainEventService ?? throw new ArgumentNullException(nameof(domainEventService));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
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
            SetAuditProperties();

            await DispatchEvents(cancellationToken);

            SetAuditProperties(); // This needs to be run again to set the audit properties of any entities that were added/modified in any Domain Event Handlers after dispatch

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
        var domainEventEntities = ChangeTracker.Entries<IHasDomainEvents>()
            .ToList();

        foreach (var domainEventEntity in domainEventEntities)
        {
            domainEventEntity.Entity.ClearDomainEvents();

            foreach (var domainEvent in domainEventEntity.Entity.DomainEvents)
            {
                await _domainEventService.Publish(domainEvent, cancellationToken);
            }
        }
    }

    private void SetAuditProperties()
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = _userService.GetUserIdentity();
                    entry.Entity.CreatedOn = DateTimeOffset.Now;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedBy = _userService.GetUserIdentity();
                    entry.Entity.ModifiedOn = DateTimeOffset.Now;
                    break;
                case EntityState.Detached:
                    break;
                case EntityState.Unchanged:
                    break;
                case EntityState.Deleted:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}