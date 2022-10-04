using FakeSurveyGenerator.Shared.SeedWork;

namespace FakeSurveyGenerator.Application.Common.DomainEvents;

public interface IDomainEventService
{
    Task Publish(DomainEvent domainEvent, CancellationToken cancellationToken);
}