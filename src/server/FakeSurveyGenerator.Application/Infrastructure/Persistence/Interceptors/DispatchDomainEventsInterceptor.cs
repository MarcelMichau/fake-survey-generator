using FakeSurveyGenerator.Application.Domain.Shared.SeedWork;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Application.Infrastructure.Persistence.Interceptors;
internal class DispatchDomainEventsInterceptor(IMediator mediator, ILogger<DispatchDomainEventsInterceptor> logger) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        DispatchDomainEvents(eventData.Context).GetAwaiter().GetResult();

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        await DispatchDomainEvents(eventData.Context);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public async Task DispatchDomainEvents(DbContext? context)
    {
        if (context == null) return;

        var entities = context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity);

        var hasDomainEventsEnumerable = entities.ToList();
        var domainEvents = hasDomainEventsEnumerable
            .SelectMany(e => e.DomainEvents)
            .ToList();

        hasDomainEventsEnumerable.ToList().ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            logger.LogInformation("Publishing Domain Event ({event})", domainEvent.GetType().Name);
            await mediator.Publish(domainEvent);
        }
    }
}
