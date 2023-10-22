using FakeSurveyGenerator.Application.Domain.Shared.SeedWork;

namespace FakeSurveyGenerator.Application.Shared.DomainEvents;

public interface IDomainEventService
{
    Task Publish(DomainEvent domainEvent, CancellationToken cancellationToken);
}