using FakeSurveyGenerator.Application.Common.DateTime;
using FakeSurveyGenerator.Application.Common.DomainEvents;
using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Data;
using FakeSurveyGenerator.Infrastructure.Persistence;
using FakeSurveyGenerator.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FakeSurveyGenerator.Application.Tests;

public static class SurveyContextFactory
{
    public static SurveyContext Create()
    {
        var mockUserService = Substitute.For<IUserService>();
        mockUserService.GetUserInfo(Arg.Any<CancellationToken>()).Returns(new TestUser());
        mockUserService.GetUserIdentity().Returns(new TestUser().Id);

        var fixedDateTime = new DateTimeOffset(3001, 1, 1, 12, 1, 1, new TimeSpan(2, 0, 0));

        var mockDateTimeService = Substitute.For<IDateTime>();
        mockDateTimeService.Now.Returns(fixedDateTime);

        var options = new DbContextOptionsBuilder<SurveyContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var auditableEntitySaveChangesInterceptor = new AuditableEntitySaveChangesInterceptor(mockUserService, mockDateTimeService);

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