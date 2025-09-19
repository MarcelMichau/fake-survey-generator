using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Application.DomainEvents;

public sealed class DomainEventPublisher(IServiceProvider serviceProvider, ILogger<DomainEventPublisher> logger)
    : IEventBus
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
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
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

        var handlers = _serviceProvider.GetServices(handlerType).ToList();

        if (handlers.Count == 0)
        {
            _logger.LogDebug("No handlers found for domain event: {EventType}", eventType.Name);
            return;
        }

        var handleTasks = handlers.Select(async handler =>
        {
            try
            {
                var method = handlerType.GetMethod(nameof(IDomainEventHandler<>.HandleAsync));
                if (method != null)
                {
                    if (handler != null)
                    {
                        _logger.LogDebug("Executing handler {HandlerType} for event {EventType}",
                            handler.GetType().Name, eventType.Name);

                        var result = method.Invoke(handler, [domainEvent, cancellationToken]);
                        if (result is Task task)
                            await task;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Handler {HandlerType} failed to process event {EventType} with ID: {EventId}", 
                    handler?.GetType().Name, eventType.Name, domainEvent.Id);
                throw;
            }
        });

        await Task.WhenAll(handleTasks);
        
        _logger.LogDebug("Processed domain event: {EventType} with {HandlerCount} handlers", 
            eventType.Name, handlers.Count);
    }
}