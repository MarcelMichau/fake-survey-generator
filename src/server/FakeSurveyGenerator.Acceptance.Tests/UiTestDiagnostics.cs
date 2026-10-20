using System.Collections.Concurrent;
using Microsoft.Playwright;

namespace FakeSurveyGenerator.Acceptance.Tests;

internal static class UiTestDiagnostics
{
    public static async Task RunAsync(IBrowserContext browserContext, Func<IPage, Task> test)
    {
        var testContext = TestContext.Current!;
        var messages = new ConcurrentQueue<string>();
        var page = await browserContext.NewPageAsync();
        void Log(string message) => messages.Enqueue($"{DateTimeOffset.UtcNow:O} {message}");

        page.Console += (_, message) => Log($"console.{message.Type}: {message.Text}");
        page.PageError += (_, error) => Log($"pageerror: {error}");
        page.RequestFailed += (_, request) => Log($"requestfailed: {request.Method} {request.Url} {request.Failure}");
        page.Response += (_, response) =>
        {
            if (response.Status >= 400) Log($"http: {response.Status} {response.Url}");
        };

        var tracing = false;
        await TryCaptureAsync("start trace", async () =>
        {
            await browserContext.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = true });
            tracing = true;
        });

        try
        {
            await test(page);
        }
        catch
        {
            // Diagnostics must never replace the original navigation/assertion failure.
            await TryCaptureAsync("failure diagnostics", async () =>
            {
                var directory = Path.Combine(TestContext.ResultsDirectory, "ui-diagnostics", Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(directory);

                async Task SaveAsync(string name, Func<string, Task> capture)
                {
                    await TryCaptureAsync(name, async () =>
                    {
                        var path = Path.Combine(directory, name);
                        await capture(path);
                        testContext.Output.AttachArtifact(path, name);
                    });
                }

                await SaveAsync("page.png", path => page.ScreenshotAsync(new() { Path = path, FullPage = true, Timeout = 5000 }));
                await SaveAsync("page.html", async path => await File.WriteAllTextAsync(path, await page.ContentAsync()));
                if (tracing)
                {
                    tracing = false;
                    await SaveAsync("trace.zip", path => browserContext.Tracing.StopAsync(new() { Path = path }));
                }

                Log($"Final page URL: {page.Url}");
                await SaveAsync("browser.log", path => File.WriteAllLinesAsync(path, messages));
            });
            throw;
        }
        finally
        {
            if (tracing) await TryCaptureAsync("discard trace", () => browserContext.Tracing.StopAsync());
        }
    }

    private static async Task TryCaptureAsync(string description, Func<Task> capture)
    {
        try
        {
            await capture().WaitAsync(TimeSpan.FromSeconds(10));
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Could not capture UI diagnostics ({description}): {exception.Message}");
        }
    }
}
