using FakeSurveyGenerator.Application.Domain.Shared;
using FakeSurveyGenerator.Application.Domain.Users;
using FakeSurveyGenerator.Application.Features.Users;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Shared.Errors;
using FakeSurveyGenerator.Application.Shared.Identity;
using FakeSurveyGenerator.Application.TestHelpers;
using FakeSurveyGenerator.Application.Tests.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NSubstitute;

namespace FakeSurveyGenerator.Application.Tests.Features.Users;

public sealed class RegisterUserEdgeCaseTests
{
    [Test]
    [Arguments("", "user@test.com")]
    [Arguments("   ", "user@test.com")]
    [Arguments("User", "")]
    [Arguments("User", "  ")]
    public async Task GivenIncompleteProfile_WhenCallingHandle_ThenProfileIncompleteErrorShouldBeReturnedAndNothingPersisted(
        string displayName, string emailAddress)
    {
        var options = new DbContextOptionsBuilder<SurveyContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var context = new SurveyContext(options);
        var sut = new RegisterUserCommandHandler(UserService(new TestUser("sub-1", displayName, emailAddress)), context);

        var result = await sut.Handle(new RegisterUserCommand());

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(Errors.General.UserProfileIncomplete());
        await Assert.That(await context.Users.AnyAsync()).IsFalse();
    }

    [Test]
    public async Task GivenUserRegisteredConcurrentlyBetweenLookupAndInsert_WhenCallingHandle_ThenExistingRegistrationShouldBeReturned()
    {
        var databaseName = Guid.NewGuid().ToString();
        var racingRegistration = new RegisterBeforeSaveInterceptor(databaseName);
        var options = new DbContextOptionsBuilder<SurveyContext>()
            .UseInMemoryDatabase(databaseName)
            .AddInterceptors(SurveyContextFactory.CreateAuditInterceptor(), racingRegistration)
            .Options;
        await using var context = new SurveyContext(options);
        var sut = new RegisterUserCommandHandler(UserService(new TestUser("sub-race", "Racer", "racer@test.com")), context);

        var result = await sut.Handle(new RegisterUserCommand());

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.IsNewRegistration).IsFalse();
        await Assert.That(result.Value.User.ExternalUserId).IsEqualTo("sub-race");
        await Assert.That(await context.Users.CountAsync(user => user.ExternalUserId == "sub-race")).IsEqualTo(1);
    }

    [Test]
    public async Task GivenInsertFailsForUnrelatedReason_WhenCallingHandle_ThenExceptionShouldPropagate()
    {
        var options = new DbContextOptionsBuilder<SurveyContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(SurveyContextFactory.CreateAuditInterceptor(), new FailingSaveInterceptor())
            .Options;
        await using var context = new SurveyContext(options);
        var sut = new RegisterUserCommandHandler(UserService(new TestUser("sub-1", "User", "user@test.com")), context);

        await Assert.That(async () => await sut.Handle(new RegisterUserCommand()))
            .ThrowsException().And.IsTypeOf<DbUpdateException>();
    }

    private static IUserService UserService(IUser user)
    {
        var userService = Substitute.For<IUserService>();
        userService.GetUserInfo(Arg.Any<CancellationToken>()).Returns(user);
        return userService;
    }

    /// <summary>
    /// Simulates another request winning the race: the same user is committed through a second context just before
    /// this save, which then fails like a unique index violation on ExternalUserId would.
    /// </summary>
    private sealed class RegisterBeforeSaveInterceptor(string databaseName) : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var options = new DbContextOptionsBuilder<SurveyContext>().UseInMemoryDatabase(databaseName)
                .AddInterceptors(SurveyContextFactory.CreateAuditInterceptor()).Options;
            await using var otherContext = new SurveyContext(options);
            otherContext.Users.Add(new User(
                NonEmptyString.Create("Racer"),
                NonEmptyString.Create("racer@test.com"),
                NonEmptyString.Create("sub-race")));
            await otherContext.SaveChangesAsync(cancellationToken);

            throw new DbUpdateException("Cannot insert duplicate key");
        }
    }

    private sealed class FailingSaveInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            throw new DbUpdateException("Database unavailable");
        }
    }
}
