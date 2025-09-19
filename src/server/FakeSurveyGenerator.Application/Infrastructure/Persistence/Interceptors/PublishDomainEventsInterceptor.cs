using FakeSurveyGenerator.Application.Domain.Shared.SeedWork;
using FakeSurveyGenerator.Application.DomainEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Application.Infrastructure.Persistence.Interceptors;

internal sealed class PublishDomainEventsInterceptor(
    IEventBus eventBus,
    ILogger<PublishDomainEventsInterceptor> logger)
    : SaveChangesInterceptor
{
    private readonly IEventBus _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    private readonly ILogger<PublishDomainEventsInterceptor> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        PublishDomainEvents(eventData.Context).GetAwaiter().GetResult();

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        await PublishDomainEvents(eventData.Context, cancellationToken);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
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
            _logger.LogInformation("Publishing domain event ({EventType})", domainEvent.GetType().Name);
            await _eventBus.PublishAsync(domainEvent, cancellationToken);
        }
    }
}
