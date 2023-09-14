using NLog;
using Tharga.Logging.Logtail;
using LogLevel = NLog.LogLevel;

var pp = new LogtailLoggerProvider();

var logger = LogManager.GetCurrentClassLogger();
logger.Log(LogLevel.Info, "Initiating Logging.Sample.Web Server.");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddControllers();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception e)
{
    Console.WriteLine($"{e.GetType().Name}: {e.Message} @{e.StackTrace}");
    logger.Log(LogLevel.Fatal, e, $"Failed to start LeftBehind Server with message {e.Message}.");
}