using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Tharga.Logging.Logtail;

internal class LogtailLogger : ILogger
{
    private static readonly SemaphoreSlim Lock = new(1, 1);
    private readonly string _categoryName;
    private readonly Logger _logger;
    private readonly LogLevel _minLogLevel;

    public LogtailLogger(IHostEnvironment hostEnvironment = null, IConfiguration configuration = null, ILoggingDefaultData loggingDefaultData = null, string categoryName = null)
    {
        _minLogLevel = Enum.TryParse(configuration?.GetSection("Logging")?.GetSection("LogLevel")?.GetSection("Default")?.Value, true, out LogLevel level) ? level : LogLevel.Information;
        _categoryName = categoryName;
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

        _logger = LogManager.GetCurrentClassLogger();

        try
        {
            Lock.Wait();

            if (!string.IsNullOrEmpty(hostEnvironment?.EnvironmentName) && !_logger.Properties.ContainsKey("environment")) _logger.Properties.TryAdd("environment", hostEnvironment.EnvironmentName);
            if (!string.IsNullOrEmpty(hostEnvironment?.ApplicationName) && !_logger.Properties.ContainsKey("application")) _logger.Properties.TryAdd("application", hostEnvironment.ApplicationName);
            if (!string.IsNullOrEmpty(version) && !_logger.Properties.ContainsKey("version")) _logger.Properties.TryAdd("version", version);
            if (!string.IsNullOrEmpty(Environment.MachineName) && !_logger.Properties.ContainsKey("machine")) _logger.Properties.TryAdd("machine", Environment.MachineName);
            if (!string.IsNullOrEmpty(Environment.UserName) && !_logger.Properties.ContainsKey("userName")) _logger.Properties.TryAdd("userName", Environment.UserName);

            if (loggingDefaultData != null)
            {
                foreach (var data in loggingDefaultData.GetData())
                {
                    if (data.Value != default && !_logger.Properties.ContainsKey(data.Key))
                    {
                        _logger.Properties.TryAdd(data.Key, data.Value);
                    }
                }
            }
        }
        finally
        {
            Lock.Release();
        }
    }

    public event EventHandler<SendLogEventArgs> BeforeSendEvent;
    public event EventHandler<SendLogEventArgs> AfterSendEvent;

    public IDisposable BeginScope<TState>(TState state)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        return ScopeContext.PushNestedState(state);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= _minLogLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        var logData = ((IEnumerable<KeyValuePair<string, object>>)state).ToLogData();

        var parsedMessage = formatter.Invoke(state, exception);
        logData.AddField("message", parsedMessage);

        var logEntry = new LogEntry
        {
            Message = parsedMessage,
            Exception = exception,
            LogLevel = logLevel,
            Data = logData
        };

        Send(logEntry);
    }

    private void Send(LogEntry logEntry)
    {
        Task.Run(() =>
        {
            logEntry = Unpack(logEntry);

            var sendLogEventArgs = new SendLogEventArgs(logEntry);
            BeforeSendEvent?.Invoke(this, sendLogEventArgs);

            if (string.IsNullOrEmpty(_categoryName))
            {
                _logger.Log(ToLevel(logEntry.LogLevel), $"{logEntry.Message} {{data}}", logEntry.GetData());
            }
            else
            {
                _logger.Log(ToLevel(logEntry.LogLevel), $"{{category}}: {logEntry.Message} {{data}}", _categoryName, logEntry.GetData());
            }

            AfterSendEvent?.Invoke(this, sendLogEventArgs);
        });
    }

    internal static LogEntry Unpack(LogEntry logEntry)
    {
        var message = logEntry.Message;

        var pos = message.IndexOf("[", StringComparison.Ordinal);
        if (pos != -1)
        {
            var dataPart = message.Substring(pos);
            AppendData(logEntry, dataPart);
            message = message.Substring(0, pos);
        }

        return logEntry with { Message = message.TrimEnd() };
    }

    private static void AppendData(LogEntry logEntry, string dataPart)
    {
        var pairs = dataPart.TrimEnd().TrimStart('[').TrimEnd(']').Split(",");

        foreach (var dup in pairs.Select(x =>
                 {
                     var dup = x.Split(":");
                     if (dup.Length == 2)
                     {
                         return (Key: dup[0].Trim(), Value: dup[1].Trim());
                     }

                     return (null, null);
                 }).Where(x => !string.IsNullOrWhiteSpace(x.Key) && !string.IsNullOrWhiteSpace(x.Value)))
        {
            logEntry.Data.AddField(dup.Key, dup.Value);
        }
    }

    private NLog.LogLevel ToLevel(LogLevel logLevel)
    {
        var result = NLog.LogLevel.FromString(logLevel.ToString());
        return result;
    }
}

internal class LogtailLogger<T> : LogtailLogger, ILogger<T>
{
    public LogtailLogger(IHostEnvironment hostEnvironment = null, IConfiguration configuration = null, ILoggingDefaultData loggingDefaultData = null)
        : base(hostEnvironment, configuration, loggingDefaultData, typeof(T).Name)
    {
    }
}