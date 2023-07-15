using System;
using Microsoft.Extensions.Logging;

namespace Tharga.Logging;

public record LogEntry
{
    public Exception Exception { get; init; }
    public string Message { get; init; }
    public LogLevel LogLevel { get; init; }
    public LogData Data { get; init; }
}