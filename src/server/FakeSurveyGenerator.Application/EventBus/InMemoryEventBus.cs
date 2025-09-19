using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Application.EventBus;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTimeOffset OccurredAt { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}

public sealed class InMemoryEventBus : IEventBus, IDisposable
{
    private readonly Channel<IDomainEvent> _channel;
    private readonly ChannelWriter<IDomainEvent> _writer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InMemoryEventBus> _logger;

    public InMemoryEventBus(IServiceProvider serviceProvider, ILogger<InMemoryEventBus> logger)
    {
        var options = new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        };
        
        _channel = Channel.CreateBounded<IDomainEvent>(options);
        _writer = _channel.Writer;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default) 
        where TEvent : IDomainEvent
    {
        await _writer.WriteAsync(domainEvent, cancellationToken);
        _logger.LogDebug("Published domain event: {EventType} with ID: {EventId}", 
            typeof(TEvent).Name, domainEvent.Id);
    }

    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _writer.WriteAsync(domainEvent, cancellationToken);
        _logger.LogDebug("Published domain event: {EventType} with ID: {EventId}", 
            domainEvent.GetType().Name, domainEvent.Id);
    }

    public ChannelReader<IDomainEvent> Reader => _channel.Reader;

    public void Dispose()
    {
        _writer.Complete();
    }
}

public sealed class DomainEventProcessor(
    InMemoryEventBus eventBus,
    IServiceProvider serviceProvider,
    ILogger<DomainEventProcessor> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var domainEvent in eventBus.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessEvent(domainEvent, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing domain event: {EventType} with ID: {EventId}", 
                    domainEvent.GetType().Name, domainEvent.Id);
            }
        }
    }

    private async Task ProcessEvent(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var eventType = domainEvent.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

        using var scope = serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices(handlerType).ToList();

        var handleTasks = handlers.Select(async handler =>
        {
            try
            {
                var method = handlerType.GetMethod(nameof(IDomainEventHandler<>.HandleAsync));
                if (method != null)
                {
                    var result = method.Invoke(handler, [domainEvent, cancellationToken]);
                    if (result is Task task)
                        await task;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Handler {HandlerType} failed to process event {EventType}", 
                    handler?.GetType().Name, eventType.Name);
                throw;
            }
        });

        await Task.WhenAll(handleTasks);
        
        logger.LogDebug("Processed domain event: {EventType} with {HandlerCount} handlers", 
            eventType.Name, handlers.Count());
    }
}