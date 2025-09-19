namespace FakeSurveyGenerator.Application.DomainEvents;

public interface IDomainEvent
{
    Guid Id { get; }
}