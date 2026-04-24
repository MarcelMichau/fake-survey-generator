using System.Net;
using System.Net.Http.Json;
using AutoFixture;
using FakeSurveyGenerator.Api.Tests.Integration.Setup;
using FakeSurveyGenerator.Application.Features.Surveys;
using FakeSurveyGenerator.Application.Features.Users;
using FakeSurveyGenerator.Application.TestHelpers;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.Api.Tests.Integration.Shared;

public sealed class ForwardedHeadersTests
{
    [ClassDataSource<IntegrationTestFixture>(Shared = SharedType.PerTestSession)]
    public required IntegrationTestFixture TestFixture { get; init; }

    private readonly TestUser _testUser = new Fixture().Create<TestUser>();

    [Test]
    public async Task GivenForwardedHeadersEnabled_WhenRequestIncludesXForwardedProtoHttps_ThenRequestIsNotRedirectedToHttps()
    {
        await using var factory = TestFixture.Factory!.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "FORWARDEDHEADERS_ENABLED", "true" }
                });
            });
            builder.ConfigureTestServices(services =>
            {
                services.Configure<HttpsRedirectionOptions>(options => options.HttpsPort = 443);
            });
        });

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        using var request = new HttpRequestMessage(HttpMethod.Get, "/openapi/v1.json");
        request.Headers.Add("X-Forwarded-Proto", "https");

        var response = await client.SendAsync(request);

        await Assert.That(response.StatusCode).IsNotEqualTo(HttpStatusCode.RedirectKeepVerb);
    }

    [Test]
    public async Task GivenForwardedHeadersEnabled_WhenRequestDoesNotIncludeXForwardedProtoHeader_ThenRequestIsRedirectedToHttps()
    {
        await using var factory = TestFixture.Factory!.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "FORWARDEDHEADERS_ENABLED", "true" }
                });
            });
            builder.ConfigureTestServices(services =>
            {
                services.Configure<HttpsRedirectionOptions>(options => options.HttpsPort = 443);
            });
        });

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/openapi/v1.json");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.RedirectKeepVerb);
    }

    [Test]
    public async Task GivenForwardedHeadersEnabled_WhenCreatingSurveyWithXForwardedHost_ThenLocationHeaderContainsForwardedHost()
    {
        const string forwardedHost = "fakesurveygenerator.mysecondarydomain.com";

        await using var factory = TestFixture.Factory!.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "FORWARDEDHEADERS_ENABLED", "true" }
                });
            });
        });

        var authenticatedClient = factory.WithSpecificUser(_testUser);

        await authenticatedClient.PostAsJsonAsync("/api/user/register", new RegisterUserCommand());

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/survey");
        request.Headers.Add("X-Forwarded-Host", forwardedHost);
        request.Content = JsonContent.Create(new CreateSurveyCommand
        {
            SurveyTopic = "Forwarded host test",
            NumberOfRespondents = 100,
            RespondentType = "Testers",
            SurveyOptions =
            [
                new SurveyOptionDto { OptionText = "Option A" },
                new SurveyOptionDto { OptionText = "Option B" }
            ]
        });

        using var response = await authenticatedClient.SendAsync(request);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
        await Assert.That(response.Headers.Location!.Host).IsEqualTo(forwardedHost);
    }

    [Test]
    public async Task GivenForwardedHeadersEnabled_WhenCreatingSurveyWithDisallowedXForwardedHost_ThenLocationHeaderContainsDefaultHost()
    {
        const string disallowedHost = "not-an-allowed-host.example.com";

        await using var factory = TestFixture.Factory!.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "FORWARDEDHEADERS_ENABLED", "true" }
                });
            });
        });

        var authenticatedClient = factory.WithSpecificUser(_testUser);

        await authenticatedClient.PostAsJsonAsync("/api/user/register", new RegisterUserCommand());

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/survey");
        request.Headers.Add("X-Forwarded-Host", disallowedHost);
        request.Content = JsonContent.Create(new CreateSurveyCommand
        {
            SurveyTopic = "Disallowed host test",
            NumberOfRespondents = 100,
            RespondentType = "Testers",
            SurveyOptions =
            [
                new SurveyOptionDto { OptionText = "Option A" },
                new SurveyOptionDto { OptionText = "Option B" }
            ]
        });

        using var response = await authenticatedClient.SendAsync(request);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
        await Assert.That(response.Headers.Location!.Host).IsEqualTo("localhost");
    }

    [Test]
    public async Task GivenForwardedHeadersEnabled_WhenCreatingSurveyWithoutXForwardedHost_ThenLocationHeaderContainsDefaultHost()
    {
        await using var factory = TestFixture.Factory!.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "FORWARDEDHEADERS_ENABLED", "true" }
                });
            });
        });

        var authenticatedClient = factory.WithSpecificUser(_testUser);

        await authenticatedClient.PostAsJsonAsync("/api/user/register", new RegisterUserCommand());

        using var response = await authenticatedClient.PostAsJsonAsync("/api/survey", new CreateSurveyCommand
        {
            SurveyTopic = "Default host test",
            NumberOfRespondents = 100,
            RespondentType = "Testers",
            SurveyOptions =
            [
                new SurveyOptionDto { OptionText = "Option A" },
                new SurveyOptionDto { OptionText = "Option B" }
            ]
        });

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
        await Assert.That(response.Headers.Location!.Host).IsEqualTo("localhost");
    }
}
