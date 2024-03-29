﻿using FakeSurveyGenerator.Application.Shared.Identity;

namespace FakeSurveyGenerator.Application.Tests.Setup;

public sealed record TestUser(string Id, string DisplayName, string EmailAddress) : IUser
{
    public TestUser() : this("test-id", "Test User", "test.user@test.com")
    {
    }
}