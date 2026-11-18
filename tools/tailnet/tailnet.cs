#!/usr/bin/env dotnet
// Publishes the locally running Aspire app as a Tailscale Service, reachable only on your tailnet.
// Usage: dotnet tools/tailnet/tailnet.cs <up|down|status> [--service <name>]
// See docs/running-on-a-tailnet.md.

using System.Diagnostics;
using System.Text.Json;

// Must match the ports in src/server/FakeSurveyGenerator.AppHost/Tailnet.cs.
(string Name, int Port)[] endpoints =
[
    ("Aspire Dashboard", 17081),
    ("UI", 3000),
    ("API", 7223),
    ("Redis Insight", 5540),
];

string? command = null;
var serviceName = "fsg";
for (var i = 0; i < args.Length; i++)
{
    if (args[i] == "--service")
    {
        if (i + 1 >= args.Length || args[i + 1].StartsWith("--") || string.IsNullOrWhiteSpace(args[i + 1]))
        {
            return Usage("--service needs a name.");
        }

        serviceName = args[++i];
    }
    else if (args[i].StartsWith("--"))
    {
        return Usage($"Unknown option {args[i]}.");
    }
    else if (command is null)
    {
        command = args[i];
    }
    else
    {
        return Usage($"Unexpected argument {args[i]}.");
    }
}

var service = $"svc:{serviceName}";

switch (command)
{
    case "up":
        var published = new List<int>();
        foreach (var (_, port) in endpoints)
        {
            // The local endpoints use the ASP.NET Core developer certificate, which Tailscale does not trust.
            if (!Tailscale("serve", $"--service={service}", $"--https={port}", $"https+insecure://127.0.0.1:{port}"))
            {
                ReportPartialUp(published);
                return 1;
            }

            published.Add(port);
        }

        var host = $"{serviceName}.{GetMagicDnsSuffix()}";
        Console.WriteLine();
        Console.WriteLine($"Published {service}. Tailnet URLs:");
        foreach (var (name, port) in endpoints)
        {
            Console.WriteLine($"  {name,-17} https://{host}:{port}");
        }

        Console.WriteLine();
        Console.WriteLine("To show these links in the dashboard, set the AppHost secret once:");
        Console.WriteLine($"  aspire secret set Tailnet:Host {host}");
        Console.WriteLine("Open the dashboard login link that aspire prints, with localhost replaced by the host above.");
        Console.WriteLine($"The ports stay published after Aspire stops. Run 'down' to stop publishing {service}.");
        break;

    case "down":
        if (!Tailscale("serve", "clear", service))
        {
            return 1;
        }

        Console.WriteLine($"Removed {service} from this device.");
        break;

    case "status":
        return Tailscale("serve", "status") ? 0 : 1;

    case null:
        return Usage(null);

    default:
        return Usage($"Unknown command {command}.");
}

return 0;

int Usage(string? error)
{
    if (error is not null)
    {
        Console.Error.WriteLine(error);
    }

    Console.Error.WriteLine("Usage: dotnet tools/tailnet/tailnet.cs <up|down|status> [--service <name>]");
    Console.Error.WriteLine("  up      Publish the app's ports on svc:<name> (default name: fsg)");
    Console.Error.WriteLine("  down    Stop publishing svc:<name> from this device");
    Console.Error.WriteLine("  status  Show the current tailscale serve configuration");
    return error is null ? 0 : 1;
}

void ReportPartialUp(List<int> publishedPorts)
{
    if (publishedPorts.Count == 0)
    {
        Console.Error.WriteLine($"No ports were published on {service}.");
        return;
    }

    // Tailscale keeps each mapping in the background, so the earlier ones are still active.
    Console.Error.WriteLine($"These ports were already published on {service} and are still active:");
    foreach (var port in publishedPorts)
    {
        Console.Error.WriteLine($"  {port}  (remove with: tailscale serve --service={service} --https={port} off)");
    }

    Console.Error.WriteLine("Fix the problem and run 'up' again, or remove the ports above.");
}

string GetMagicDnsSuffix()
{
    using var status = JsonDocument.Parse(TailscaleOutput("status", "--json"));
    return status.RootElement.GetProperty("CurrentTailnet").GetProperty("MagicDNSSuffix").GetString()
        ?? throw new InvalidOperationException("MagicDNS is not enabled on this tailnet.");
}

bool Tailscale(params string[] arguments) => RunTailscale(captureOutput: false, arguments).ExitCode == 0;

string TailscaleOutput(params string[] arguments)
{
    var (exitCode, output) = RunTailscale(captureOutput: true, arguments);
    return exitCode == 0
        ? output
        : throw new InvalidOperationException($"tailscale {string.Join(' ', arguments)} failed.");
}

(int ExitCode, string Output) RunTailscale(bool captureOutput, string[] arguments)
{
    var startInfo = new ProcessStartInfo(FindTailscale()) { RedirectStandardOutput = captureOutput };
    foreach (var argument in arguments)
    {
        startInfo.ArgumentList.Add(argument);
    }

    using var process = Process.Start(startInfo)
        ?? throw new InvalidOperationException("Could not start the tailscale CLI.");
    var output = captureOutput ? process.StandardOutput.ReadToEnd() : "";
    process.WaitForExit();

    if (process.ExitCode != 0)
    {
        Console.Error.WriteLine($"tailscale {string.Join(' ', arguments)} failed with exit code {process.ExitCode}.");
    }

    return (process.ExitCode, output);
}

static string FindTailscale()
{
    if (Environment.GetEnvironmentVariable("TAILSCALE_CLI") is { Length: > 0 } configured)
    {
        return configured;
    }

    // The macOS app does not put its CLI on PATH unless you install it from the app's settings.
    const string macAppCli = "/Applications/Tailscale.app/Contents/MacOS/Tailscale";
    return OperatingSystem.IsMacOS() && File.Exists(macAppCli) && !IsOnPath("tailscale") ? macAppCli : "tailscale";
}

static bool IsOnPath(string executable) =>
    (Environment.GetEnvironmentVariable("PATH") ?? "")
        .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
        .Any(directory => File.Exists(Path.Combine(directory, executable)));
