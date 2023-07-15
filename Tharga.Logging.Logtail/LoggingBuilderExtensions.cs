using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Tharga.Logging.Logtail;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder AddLogtailLogger(this ILoggingBuilder builder)
    {
        builder.Services.AddSingleton<ILoggerProvider, LogtailLoggerProvider>();
        return builder;
    }
}