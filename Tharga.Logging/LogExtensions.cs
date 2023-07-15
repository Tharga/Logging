using System.Linq;
using Microsoft.Extensions.Logging;

namespace Tharga.Logging;

public static class LogExtensions
{
    public static void LogAction(this ILogger logger, LogLevel logLevel, string action, string message, params object[] args)
    {
        var parameters = new []{ action }.Union(args);
        logger.Log(logLevel, $"{{action}}: {message}", parameters);
    }

    public static void LogAction(this ILogger logger, LogLevel logLevel, string action, LogData data, string message, params object[] args)
    {
        var parameters = new[] { action }.Union(args).Union(new[] { data.GetData() });
        logger.Log(logLevel, $"{{action}}: {message} {{details}}", parameters);
    }

    public static void LogAction(this ILogger logger, LogLevel logLevel, string action, string operation, string message, params object[] args)
    {
        var parameters = new[] { action, operation }.Union(args);
        logger.Log(logLevel, $"{{action}}.{{operation}}: {message}", parameters);
    }

    public static void LogAction(this ILogger logger, LogLevel logLevel, string action, string operation, LogData data, string message, params object[] args)
    {
        var parameters = new[] { action }.Union(args).Union(new[] { data.GetData() });
        logger.Log(logLevel, $"{{action}}.{{operation}}: {message} {{details}}", parameters);
    }

    public static void LogData(this ILogger logger, LogLevel logLevel, LogData data, string message, params object[] args)
    {
        var parameters = args.Union(new[] { data.GetData() });
        logger.Log(logLevel, $"{message} {{details}}", parameters);
    }
}