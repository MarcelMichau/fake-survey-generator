using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Application.DomainEvents;

public sealed class DomainEventPublisher(
    IEnumerable<IDomainEventHandler> handlers,
    ILogger<DomainEventPublisher> logger)
    : IEventBus
{
    private readonly IReadOnlyList<IDomainEventHandler> _handlers =
        (handlers ?? throw new ArgumentNullException(nameof(handlers))).ToList();
    private readonly ILogger<DomainEventPublisher> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        _logger.LogDebug("Publishing domain event: {EventType} with ID: {EventId}",
            typeof(TEvent).Name, domainEvent.Id);

        await ProcessEvent(domainEvent, cancellationToken);
    }

    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Publishing domain event: {EventType} with ID: {EventId}",
            domainEvent.GetType().Name, domainEvent.Id);

        await ProcessEvent(domainEvent, cancellationToken);
    }

    private async Task ProcessEvent(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var eventType = domainEvent.GetType();
        var handlers = _handlers.Where(handler => handler.EventType == eventType).ToList();

        if (handlers.Count == 0)
        {
            _logger.LogDebug("No handlers found for domain event: {EventType}", eventType.Name);
            return;
        }

        await Task.WhenAll(handlers.Select(handler => InvokeHandler(handler, domainEvent, cancellationToken)));

        _logger.LogDebug("Processed domain event: {EventType} with {HandlerCount} handlers",
            eventType.Name, handlers.Count);
    }

    private async Task InvokeHandler(
        IDomainEventHandler handler,
        IDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Executing handler {HandlerType} for event {EventType}",
                handler.GetType().Name, domainEvent.GetType().Name);

            await handler.HandleAsync(domainEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Handler {HandlerType} failed to process event {EventType} with ID: {EventId}",
                handler.GetType().Name, domainEvent.GetType().Name, domainEvent.Id);
            throw;
        }
    }
}
