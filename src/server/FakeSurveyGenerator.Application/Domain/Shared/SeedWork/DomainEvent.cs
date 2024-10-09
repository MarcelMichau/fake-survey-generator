using MediatR;

namespace FakeSurveyGenerator.Application.Domain.Shared.SeedWork;

public abstract class DomainEvent : INotification
{
    public DateTimeOffset DateOccurred { get; protected set; } = DateTimeOffset.Now;
}