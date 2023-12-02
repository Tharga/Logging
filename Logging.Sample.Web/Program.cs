using NLog;
using Tharga.Logging.Logtail;
using LogLevel = NLog.LogLevel;

namespace Logging.Sample.Web;

public static class Program
{
    public static void Main(string[] args)
    {
        var logger = LogManager.GetCurrentClassLogger();

        try
        {
            logger.Log(LogLevel.Info, "Initiating Server.");
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception e)
        {
            Console.WriteLine($"{e.GetType().Name}: {e.Message} @{e.StackTrace}");
            logger.Log(LogLevel.Fatal, e, $"Failed to start Server with message {e.Message}.");
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.AddLogtailLogger();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Logging.Sample.Web.Startup>();
            });

        return host;
    }
}