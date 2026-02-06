using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Api.Tests.Integration.Setup;

public sealed class TestLogSink
{
  public static TestLogSink Shared { get; } = new();

  private readonly ConcurrentQueue<TestLogEntry> _entries = new();

  public IReadOnlyList<TestLogEntry> Entries => _entries.ToArray();

  public void Add(TestLogEntry entry)
  {
    _entries.Enqueue(entry);
  }

  public void Clear()
  {
    while (_entries.TryDequeue(out _))
    {
    }
  }
}

public sealed record TestLogEntry(
    string Category,
    LogLevel Level,
    EventId EventId,
    string Message,
    IReadOnlyList<KeyValuePair<string, object?>>? State,
    Exception? Exception);

public sealed class TestLoggerProvider(TestLogSink sink) : ILoggerProvider
{
  private readonly TestLogSink _sink = sink ?? throw new ArgumentNullException(nameof(sink));

  public ILogger CreateLogger(string categoryName)
  {
    return new TestLogger(categoryName, _sink);
  }

  public void Dispose()
  {
  }

  private sealed class TestLogger(string categoryName, TestLogSink sink) : ILogger
  {
    private readonly string _categoryName = categoryName;
    private readonly TestLogSink _sink = sink;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
      return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
      return logLevel != LogLevel.None;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
      if (!IsEnabled(logLevel))
      {
        return;
      }

      var message = formatter(state, exception);
      var stateValues = state as IReadOnlyList<KeyValuePair<string, object?>>;

      _sink.Add(new TestLogEntry(_categoryName, logLevel, eventId, message, stateValues, exception));
    }
  }
}
