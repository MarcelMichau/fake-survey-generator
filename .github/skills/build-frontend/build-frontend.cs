using System;
using System.Diagnostics;
using System.IO;

/// <summary>
/// Frontend build helper script - file-based app
/// Runs npm build in src/client/frontend/, outputs errors to stderr
/// Exit codes: 0 = success, 1 = build failure
/// Usage: dotnet build-frontend.cs
/// </summary>
/// 
var exitCode = 0;

try
{
  var repoRoot = FindRepositoryRoot();
  var frontendDir = Path.Combine(repoRoot, "src", "client", "frontend");

  if (!Directory.Exists(frontendDir))
  {
    Console.Error.WriteLine($"[BUILD] Frontend directory not found: {frontendDir}");
    Environment.Exit(1);
  }

  Console.WriteLine($"[BUILD] Frontend build starting from {frontendDir}");

  // If dependencies are not installed, attempt a quick install to prevent build errors
  var nodeModules = Path.Combine(frontendDir, "node_modules");
  if (!Directory.Exists(nodeModules))
  {
    Console.WriteLine("[BUILD] node_modules not found. Running 'npm ci'...");
    var ciCode = RunNpm(frontendDir, "ci");
    if (ciCode != 0)
    {
      Environment.Exit(ciCode);
    }
  }

  // Run the actual build
  exitCode = RunNpm(frontendDir, "run build");

  if (exitCode != 0)
  {
    Console.Error.WriteLine($"[BUILD] Frontend build failed with exit code {exitCode}");
  }
  else
  {
    Console.WriteLine("[BUILD] Frontend build completed successfully");
  }
}
catch (Exception ex)
{
  Console.Error.WriteLine($"[BUILD] Error during frontend build: {ex.Message}");
  Console.Error.WriteLine(ex.StackTrace);
  exitCode = 1;
}

Environment.Exit(exitCode);

static string FindRepositoryRoot()
{
  var current = new DirectoryInfo(Directory.GetCurrentDirectory());

  while (current != null)
  {
    if (File.Exists(Path.Combine(current.FullName, "FakeSurveyGenerator.slnx")) ||
        File.Exists(Path.Combine(current.FullName, "FakeSurveyGenerator.sln")))
    {
      return current.FullName;
    }
    current = current.Parent;
  }

  return Directory.GetCurrentDirectory();
}

static int RunNpm(string workingDir, string arguments)
{
  // On Windows, launching plain 'npm' can fail because it's a .cmd shim.
  // Use 'cmd.exe /c npm <args>' for reliability. On non-Windows, run 'npm' directly.
  var isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);

  var startInfo = isWindows
    ? new ProcessStartInfo
    {
      FileName = "cmd.exe",
      Arguments = "/c npm " + arguments,
      WorkingDirectory = workingDir,
      UseShellExecute = false,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      CreateNoWindow = true
    }
    : new ProcessStartInfo
    {
      FileName = "npm",
      Arguments = arguments,
      WorkingDirectory = workingDir,
      UseShellExecute = false,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      CreateNoWindow = true
    };

  using var process = new Process { StartInfo = startInfo };

  process.OutputDataReceived += (sender, e) =>
  {
    if (!string.IsNullOrEmpty(e.Data)) Console.Out.WriteLine(e.Data);
  };
  process.ErrorDataReceived += (sender, e) =>
  {
    if (!string.IsNullOrEmpty(e.Data)) Console.Error.WriteLine(e.Data);
  };

  try
  {
    process.Start();
  }
  catch (System.ComponentModel.Win32Exception ex)
  {
    // Fallback: try invoking the npm.cmd directly on Windows
    if (isWindows)
    {
      Console.Error.WriteLine($"[BUILD] Failed to start npm via cmd.exe ({ex.Message}). Retrying with npm.cmd...");
      var fallback = new ProcessStartInfo
      {
        FileName = "npm.cmd",
        Arguments = arguments,
        WorkingDirectory = workingDir,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
      };
      using var p2 = new Process { StartInfo = fallback };
      p2.OutputDataReceived += (s, e2) => { if (!string.IsNullOrEmpty(e2.Data)) Console.Out.WriteLine(e2.Data); };
      p2.ErrorDataReceived += (s, e2) => { if (!string.IsNullOrEmpty(e2.Data)) Console.Error.WriteLine(e2.Data); };
      p2.Start();
      p2.BeginOutputReadLine();
      p2.BeginErrorReadLine();
      p2.WaitForExit();
      return p2.ExitCode;
    }

    Console.Error.WriteLine($"[BUILD] Error starting npm: {ex.Message}");
    return 1;
  }

  process.BeginOutputReadLine();
  process.BeginErrorReadLine();
  process.WaitForExit();
  return process.ExitCode;
}
