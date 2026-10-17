using Microsoft.Extensions.Logging;

namespace CookieCommerce.Web.Tests.Fixtures;

public sealed class CapturingLogger<T> : ILogger<T>
{
    private readonly List<LogEntry> entries = [];

    public IReadOnlyList<LogEntry> Entries => entries;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter) => entries.Add(new LogEntry(logLevel, formatter(state, exception)));
}

public sealed record LogEntry(LogLevel Level, string Message);
