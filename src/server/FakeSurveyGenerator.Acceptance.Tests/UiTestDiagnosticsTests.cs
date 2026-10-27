using System.IO.Compression;
using TUnit.Playwright;

namespace FakeSurveyGenerator.Acceptance.Tests;

// Shares the "browser" key with EndToEndTests; see the note there.
[NotInParallel("browser")]
public sealed class UiTestDiagnosticsTests : ContextTest
{
    // Reuse browser installation and startup ordering from the other acceptance tests.
    [ClassDataSource<AcceptanceTestFixture>(Shared = SharedType.PerTestSession)]
    public required AcceptanceTestFixture TestFixture { get; init; }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenUiFailure_WhenCapturingDiagnostics_ThenArtifactsAreAttachedAndOriginalFailureIsPreserved(bool closePage)
    {
        var expected = new InvalidOperationException("Intentional failure to verify UI diagnostics");
        Exception? actual = null;
        try
        {
            await UiTestDiagnostics.RunAsync(Context, async page =>
            {
                await page.SetContentAsync("<h1>Diagnostics regression test</h1>");
                await page.EvaluateAsync("console.error('diagnostics-test-console')");
                await page.AddScriptTagAsync(new() { Content = "throw new Error('diagnostics-test-pageerror')" });
                await page.RouteAsync("**/diagnostics-failed-request", route => route.AbortAsync());
                await page.EvaluateAsync("fetch('https://example.invalid/diagnostics-failed-request').catch(() => {})");
                if (closePage) await page.CloseAsync();
                throw expected;
            });
        }
        catch (Exception exception)
        {
            actual = exception;
        }

        await Assert.That(ReferenceEquals(actual, expected)).IsTrue();
        var artifacts = TestContext.Current!.Output.Artifacts.ToDictionary(artifact => artifact.DisplayName);
        var log = await File.ReadAllTextAsync(artifacts["browser.log"].File.FullName);
        await Assert.That(log).Contains("diagnostics-test-console");
        await Assert.That(log).Contains("diagnostics-test-pageerror");
        await Assert.That(log).Contains("requestfailed:");
        using var trace = ZipFile.OpenRead(artifacts["trace.zip"].File.FullName);
        await Assert.That(trace.Entries.Any(entry => entry.FullName.EndsWith(".trace"))).IsTrue();
        if (!closePage)
        {
            await Assert.That(artifacts["page.png"].File.Length).IsPositive();
            await Assert.That(await File.ReadAllTextAsync(artifacts["page.html"].File.FullName))
                .Contains("Diagnostics regression test");
        }
    }

    [Test]
    public async Task GivenSuccessfulUiTest_WhenCompleting_ThenNoDiagnosticsAreAttached()
    {
        await UiTestDiagnostics.RunAsync(Context, page => page.SetContentAsync("<h1>Success</h1>"));
        await Assert.That(TestContext.Current!.Output.Artifacts.Count).IsEqualTo(0);
    }
}
