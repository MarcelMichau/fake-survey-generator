using System.Collections.Concurrent;
using FakeSurveyGenerator.Application.Domain.Shared;
using FakeSurveyGenerator.Application.Domain.Surveys;
using FakeSurveyGenerator.Application.Domain.Users;
using FakeSurveyGenerator.Application.DomainEvents;
using FakeSurveyGenerator.Application.Features.Notifications;
using FakeSurveyGenerator.Application.Features.Surveys;
using FakeSurveyGenerator.Application.Shared.Notifications;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FakeSurveyGenerator.Application.Tests.DomainEvents;

public sealed class DomainEventPublisherTests
{
    [Test]
    public async Task PublishAsync_DispatchesToEveryMatchingHandlerAndSkipsOtherEventTypes()
    {
        var domainEvent = new TestEvent(Guid.NewGuid());
        var cancellationToken = new CancellationTokenSource().Token;
        var receivedEventIds = new ConcurrentBag<Guid>();
        var receivedCancellationTokens = new ConcurrentBag<CancellationToken>();
        var otherEventHandlerWasCalled = false;

        var handlers = new IDomainEventHandler[]
        {
            new TestEventHandler<TestEvent>((received, token) =>
            {
                receivedEventIds.Add(received.Id);
                receivedCancellationTokens.Add(token);
                return Task.CompletedTask;
            }),
            new TestEventHandler<TestEvent>((received, token) =>
            {
                receivedEventIds.Add(received.Id);
                receivedCancellationTokens.Add(token);
                return Task.CompletedTask;
            }),
            new TestEventHandler<OtherTestEvent>((_, _) =>
            {
                otherEventHandlerWasCalled = true;
                return Task.CompletedTask;
            })
        };
        IEventBus publisher = new DomainEventPublisher(handlers, Substitute.For<ILogger<DomainEventPublisher>>());

        await publisher.PublishAsync((IDomainEvent)domainEvent, cancellationToken);

        await Assert.That(receivedEventIds.Count).IsEqualTo(2);
        await Assert.That(receivedEventIds.All(id => id == domainEvent.Id)).IsTrue();
        await Assert.That(receivedCancellationTokens.All(token => token == cancellationToken)).IsTrue();
        await Assert.That(otherEventHandlerWasCalled).IsFalse();
    }

    [Test]
    public async Task PublishAsync_SurveyCreatedEvent_SendsNotificationThroughRegisteredHandler()
    {
        var notificationService = Substitute.For<INotificationService>();
        var survey = new Survey(
            new User(
                NonEmptyString.Create("Owner"),
                NonEmptyString.Create("owner@example.test"),
                NonEmptyString.Create("owner-id")),
            NonEmptyString.Create("Test topic"),
            1,
            NonEmptyString.Create("Test respondents"));
        var domainEvent = new SurveyCreatedDomainEvent(survey);
        var cancellationToken = new CancellationTokenSource().Token;
        var handler = new SendNotificationWhenSurveyCreatedDomainEventHandler(notificationService);
        var publisher = new DomainEventPublisher(
            [handler],
            Substitute.For<ILogger<DomainEventPublisher>>());

        await publisher.PublishAsync(domainEvent, cancellationToken);

        await notificationService.Received(1).SendMessage(
            Arg.Is<MessageModel>(message =>
                message.From == "System" &&
                message.To == "Whom It May Concern" &&
                message.Subject == "New Survey Created" &&
                message.Body == $"Survey with ID: {survey.Id} created"),
            cancellationToken);
    }

    [Test]
    public async Task PublishAsync_WhenNoHandlerMatches_CompletesWithoutInvokingHandlers()
    {
        var publisher = new DomainEventPublisher(
            Array.Empty<IDomainEventHandler>(),
            Substitute.For<ILogger<DomainEventPublisher>>());

        await publisher.PublishAsync(new TestEvent(Guid.NewGuid()));
    }

    private sealed record TestEvent(Guid Id) : IDomainEvent;
    private sealed record OtherTestEvent(Guid Id) : IDomainEvent;

    private sealed class TestEventHandler<TEvent>(Func<TEvent, CancellationToken, Task> handle)
        : IDomainEventHandler<TEvent> where TEvent : IDomainEvent
    {
        public Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default)
        {
            return handle(domainEvent, cancellationToken);
        }
    }
}
