using System;
using System.Diagnostics;
using System.IO;

/// <summary>
/// Backend build helper script - file-based app
/// Runs dotnet build for all backend projects, outputs errors to stderr
/// Exit codes: 0 = success, 1 = build failure
/// Usage: dotnet build.cs
/// </summary>
/// 
var exitCode = 0;

try
{
  var repoRoot = FindRepositoryRoot();

  Console.WriteLine($"[BUILD] Backend build starting from {repoRoot}");

  var process = new Process
  {
    StartInfo = new ProcessStartInfo
    {
      FileName = "dotnet",
      Arguments = "build",
      WorkingDirectory = repoRoot,
      UseShellExecute = false,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      CreateNoWindow = true
    }
  };

  process.OutputDataReceived += (sender, e) =>
  {
    if (!string.IsNullOrEmpty(e.Data))
    {
      Console.Out.WriteLine(e.Data);
    }
  };

  process.ErrorDataReceived += (sender, e) =>
  {
    if (!string.IsNullOrEmpty(e.Data))
    {
      Console.Error.WriteLine(e.Data);
    }
  };

  process.Start();
  process.BeginOutputReadLine();
  process.BeginErrorReadLine();
  process.WaitForExit();

  exitCode = process.ExitCode;

  if (exitCode != 0)
  {
    Console.Error.WriteLine($"[BUILD] Backend build failed with exit code {exitCode}");
  }
  else
  {
    Console.WriteLine("[BUILD] Backend build completed successfully");
  }
}
catch (Exception ex)
{
  Console.Error.WriteLine($"[BUILD] Error during backend build: {ex.Message}");
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
