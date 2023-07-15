using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Tharga.Logging.Logtail;

[ProviderAlias("LogtailLogger")]
public class LogtailLoggerProvider : ILoggerProvider
{
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IConfiguration _configuration;
    private readonly ILoggingDefaultData _loggingDefaultData;

    public LogtailLoggerProvider()
    {
    }

    public LogtailLoggerProvider(IHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }

    public LogtailLoggerProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public LogtailLoggerProvider(IHostEnvironment hostEnvironment, IConfiguration configuration)
    {
        _hostEnvironment = hostEnvironment;
        _configuration = configuration;
    }

    public LogtailLoggerProvider(IHostEnvironment hostEnvironment, IConfiguration configuration, ILoggingDefaultData loggingDefaultData)
    {
        _hostEnvironment = hostEnvironment;
        _configuration = configuration;
        _loggingDefaultData = loggingDefaultData;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new LogtailLogger(_hostEnvironment, _configuration, _loggingDefaultData, categoryName);
    }

    public void Dispose()
    {
    }
}