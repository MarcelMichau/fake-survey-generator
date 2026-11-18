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

var command = args.FirstOrDefault(a => !a.StartsWith("--"));
var serviceName = GetOption("--service") ?? "fsg";
var service = $"svc:{serviceName}";

switch (command)
{
    case "up":
        foreach (var (name, port) in endpoints)
        {
            // The local endpoints use the ASP.NET Core developer certificate, which Tailscale does not trust.
            Tailscale("serve", $"--service={service}", $"--https={port}", $"https+insecure://127.0.0.1:{port}");
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
        break;

    case "down":
        Tailscale("serve", "clear", service);
        Console.WriteLine($"Removed {service} from this device.");
        break;

    case "status":
        Tailscale("serve", "status");
        break;

    default:
        Console.Error.WriteLine("Usage: dotnet tools/tailnet/tailnet.cs <up|down|status> [--service <name>]");
        Console.Error.WriteLine("  up      Publish the app's ports on svc:<name> (default name: fsg)");
        Console.Error.WriteLine("  down    Stop publishing svc:<name> from this device");
        Console.Error.WriteLine("  status  Show the current tailscale serve configuration");
        return command is null ? 0 : 1;
}

return 0;

string? GetOption(string name)
{
    var index = Array.IndexOf(args, name);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}

string GetMagicDnsSuffix()
{
    using var status = JsonDocument.Parse(TailscaleOutput("status", "--json"));
    return status.RootElement.GetProperty("CurrentTailnet").GetProperty("MagicDNSSuffix").GetString()
        ?? throw new InvalidOperationException("MagicDNS is not enabled on this tailnet.");
}

void Tailscale(params string[] arguments) => RunTailscale(captureOutput: false, arguments);
string TailscaleOutput(params string[] arguments) => RunTailscale(captureOutput: true, arguments);

string RunTailscale(bool captureOutput, string[] arguments)
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
        Environment.Exit(process.ExitCode);
    }

    return output;
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
