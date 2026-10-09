using FakeSurveyGenerator.Application.Domain.Shared;
using FakeSurveyGenerator.Application.Domain.Surveys;
using FakeSurveyGenerator.Application.Domain.Users;
using FakeSurveyGenerator.Application.DomainEvents;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Infrastructure.Persistence.Interceptors;
using FakeSurveyGenerator.Application.Tests.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FakeSurveyGenerator.Application.Tests.Infrastructure.Persistence;

public sealed class PublishDomainEventsInterceptorTests
{
    [Test]
    public async Task GivenSuccessfulSave_WhenSavingChanges_ThenEventIsPublishedAfterChangesAreCommitted()
    {
        var databaseName = Guid.NewGuid().ToString();
        var surveysVisibleWhenPublished = -1;
        var eventBus = Substitute.For<IEventBus>();
        eventBus.PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>()).Returns(async _ =>
        {
            // A separate context only sees the Survey once it has been committed.
            await using var observer = new SurveyContext(Options(databaseName));
            surveysVisibleWhenPublished = await observer.Surveys.CountAsync();
        });
        await using var context = CreateContext(databaseName, eventBus);

        await context.Surveys.AddAsync(NewSurvey());
        await context.SaveChangesAsync();

        await eventBus.Received(1).PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
        await Assert.That(surveysVisibleWhenPublished).IsEqualTo(1);
    }

    [Test]
    public async Task GivenFailedSave_WhenSavingChanges_ThenNoEventIsPublishedAndEventIsKeptForTheNextSave()
    {
        var databaseName = Guid.NewGuid().ToString();
        var eventBus = Substitute.For<IEventBus>();
        var failNextSave = new FailOnceInterceptor();
        await using var context = CreateContext(databaseName, eventBus, failNextSave);
        var survey = NewSurvey();
        await context.Surveys.AddAsync(survey);

        await Assert.That(async () => await context.SaveChangesAsync()).ThrowsException()
            .And.IsTypeOf<DbUpdateException>();
        await eventBus.DidNotReceiveWithAnyArgs().PublishAsync(default(IDomainEvent)!, default);
        await Assert.That(survey.DomainEvents.Count).IsEqualTo(1);

        await context.SaveChangesAsync();

        await eventBus.Received(1).PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
        await Assert.That(survey.DomainEvents.Count).IsEqualTo(0);
    }

    [Test]
    public async Task GivenEventHandlerThrows_WhenSavingChanges_ThenSaveStillSucceedsAndChangesArePersisted()
    {
        var databaseName = Guid.NewGuid().ToString();
        var eventBus = Substitute.For<IEventBus>();
        eventBus.PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("notification service down")));
        await using var context = CreateContext(databaseName, eventBus);

        await context.Surveys.AddAsync(NewSurvey());
        var saved = await context.SaveChangesAsync();

        await using var observer = new SurveyContext(Options(databaseName));
        await Assert.That(saved).IsGreaterThan(0);
        await Assert.That(await observer.Surveys.CountAsync()).IsEqualTo(1);
    }

    private static DbContextOptions<SurveyContext> Options(string databaseName,
        params IInterceptor[] interceptors) =>
        new DbContextOptionsBuilder<SurveyContext>()
            .UseInMemoryDatabase(databaseName)
            .AddInterceptors(interceptors)
            .Options;

    private static SurveyContext CreateContext(string databaseName, IEventBus eventBus,
        params IInterceptor[] extraInterceptors)
    {
        var publisher = new PublishDomainEventsInterceptor(eventBus,
            NullLogger<PublishDomainEventsInterceptor>.Instance);
        return new SurveyContext(Options(databaseName,
            [SurveyContextFactory.CreateAuditInterceptor(), publisher, .. extraInterceptors]));
    }

    private static Survey NewSurvey()
    {
        var owner = new User(NonEmptyString.Create("Owner"), NonEmptyString.Create("owner@test.com"),
            NonEmptyString.Create("owner-id"));
        var survey = new Survey(owner, NonEmptyString.Create("Topic"), 10, NonEmptyString.Create("Testers"));
        survey.AddSurveyOption(NonEmptyString.Create("Option"));
        return survey;
    }

    private sealed class FailOnceInterceptor : SaveChangesInterceptor
    {
        private bool _failed;

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (_failed) return base.SavingChangesAsync(eventData, result, cancellationToken);

            _failed = true;
            throw new DbUpdateException("Simulated database failure");
        }
    }
}
