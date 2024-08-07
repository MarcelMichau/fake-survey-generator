﻿using FakeSurveyGenerator.Application.Shared.Identity;

namespace FakeSurveyGenerator.Application.TestHelpers;

public sealed record TestUser(string Id, string DisplayName, string EmailAddress) : IUser
{
    public static readonly TestUser Instance = new();

    public TestUser() : this("test-id", "Test User", "test.user@test.com")
    {
    }
}