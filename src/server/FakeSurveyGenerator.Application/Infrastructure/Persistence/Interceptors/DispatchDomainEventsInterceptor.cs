using FakeSurveyGenerator.Application.Domain.Shared.SeedWork;
using FakeSurveyGenerator.Application.EventBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Application.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor that publishes domain events using the event bus
/// </summary>
public sealed class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<DispatchDomainEventsInterceptor> _logger;

    public DispatchDomainEventsInterceptor(
        IEventBus eventBus,
        ILogger<DispatchDomainEventsInterceptor> logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        DispatchDomainEvents(eventData.Context).GetAwaiter().GetResult();

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        await DispatchDomainEvents(eventData.Context, cancellationToken);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchDomainEvents(DbContext? context, CancellationToken cancellationToken = default)
    {
        if (context == null) return;

        var entities = context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        // Collect all events
        var domainEvents = entities.SelectMany(e => e.DomainEvents).ToList();

        // Clear all events from entities
        entities.ForEach(e => e.ClearDomainEvents());

        // Dispatch events
        foreach (var domainEvent in domainEvents)
        {
            _logger.LogInformation("Publishing domain event ({EventType})", domainEvent.GetType().Name);
            await _eventBus.PublishAsync(domainEvent, cancellationToken);
        }
    }
}
