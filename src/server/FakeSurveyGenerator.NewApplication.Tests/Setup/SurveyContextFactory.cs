using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Infrastructure.Persistence.Interceptors;
using FakeSurveyGenerator.Application.Shared.DomainEvents;
using FakeSurveyGenerator.Application.Shared.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace FakeSurveyGenerator.Application.Tests.Setup;

public static class SurveyContextFactory
{
    public static SurveyContext Create()
    {
        var mockUserService = Substitute.For<IUserService>();
        mockUserService.GetUserInfo(Arg.Any<CancellationToken>()).Returns(new TestUser());
        mockUserService.GetUserIdentity().Returns(new TestUser().Id);

        var fixedDateTime = new DateTimeOffset(3001, 1, 1, 12, 1, 1, new TimeSpan(2, 0, 0));

        var fakeTimeProvider = new FakeTimeProvider();
        fakeTimeProvider.SetUtcNow(fixedDateTime);

        var options = new DbContextOptionsBuilder<SurveyContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var auditableEntitySaveChangesInterceptor = new AuditableEntitySaveChangesInterceptor(mockUserService, fakeTimeProvider);

        var context = new SurveyContext(options, Substitute.For<IDomainEventService>(), auditableEntitySaveChangesInterceptor, new NullLogger<SurveyContext>());

        context.Database.EnsureCreated();

        return context;
    }

    public static async Task SeedSampleData(SurveyContext context)
    {
        await DatabaseSeed.SeedSampleData(context);
    }

    public static void Destroy(SurveyContext context)
    {
        context.Database.EnsureDeleted();

        context.Dispose();
    }
}