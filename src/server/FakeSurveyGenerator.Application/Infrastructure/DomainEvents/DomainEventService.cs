using FakeSurveyGenerator.Application.Domain.Shared.SeedWork;
using FakeSurveyGenerator.Application.Shared.DomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Application.Infrastructure.DomainEvents;

internal sealed class DomainEventService(ILogger<DomainEventService> logger, IMediator mediator) : IDomainEventService
{
    private readonly ILogger<DomainEventService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    public async Task Publish(DomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Publishing Domain Event. Event - {event}", domainEvent.GetType().Name);
        await _mediator.Publish(GetNotificationCorrespondingToDomainEvent(domainEvent), cancellationToken);
    }

    private static INotification GetNotificationCorrespondingToDomainEvent(DomainEvent domainEvent)
    {
        return Activator.CreateInstance(
                   typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType()), domainEvent) as
               INotification ??
               throw new InvalidOperationException(
                   $"Could not create instance of GenericType: {domainEvent.GetType()}");
    }
}