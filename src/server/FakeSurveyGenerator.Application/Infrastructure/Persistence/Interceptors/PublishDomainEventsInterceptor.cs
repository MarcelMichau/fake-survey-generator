using FakeSurveyGenerator.Application.Domain.Shared.SeedWork;
using FakeSurveyGenerator.Application.DomainEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Application.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Publishes the domain events raised by entities once their changes have been committed, so that nothing is
/// announced for changes which were never persisted. Events stay on the entities until the save succeeds, which means
/// a failed save does not lose them.
/// </summary>
/// <remarks>
/// Publishing happens after the commit, so a failing event handler cannot roll the change back. Handler failures are
/// logged rather than rethrown: the change is already durable and reporting the request as failed would invite a retry
/// which duplicates it. Handlers needing guaranteed delivery require an outbox.
/// </remarks>
internal sealed class PublishDomainEventsInterceptor(
    IEventBus eventBus,
    ILogger<PublishDomainEventsInterceptor> logger)
    : SaveChangesInterceptor
{
    private readonly IEventBus _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    private readonly ILogger<PublishDomainEventsInterceptor> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        PublishDomainEvents(eventData.Context).GetAwaiter().GetResult();

        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        await PublishDomainEvents(eventData.Context, cancellationToken);

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task PublishDomainEvents(DbContext? context, CancellationToken cancellationToken = default)
    {
        if (context == null) return;

        var entities = context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities.SelectMany(e => e.DomainEvents).ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            try
            {
                _logger.LogInformation("Publishing domain event ({EventType})", domainEvent.GetType().Name);
                await _eventBus.PublishAsync(domainEvent, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to publish domain event ({EventType}) after its changes were committed",
                    domainEvent.GetType().Name);
            }
        }
    }
}
