using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Tharga.Logging;

public static class MeasureExtensions
{
    public static T Measure<T>(this ILogger logger, string action, string operation, Func<MeasurementLogData, T> func, LogLevel level = LogLevel.Information, LogData logData = null)
    {
        return logger.MeasureAsync(action, operation, data =>
        {
            var result = func.Invoke(data);
            return Task.FromResult(result);
        }, level, logData).GetAwaiter().GetResult();
    }

    public static T Measure<T>(this ILogger logger, string action, Func<MeasurementLogData, T> func, LogLevel level = LogLevel.Information, LogData logData = null)
    {
        return logger.MeasureAsync(action, null, data =>
        {
            var result = func.Invoke(data);
            return Task.FromResult(result);
        }, level, logData).GetAwaiter().GetResult();
    }

    public static void Measure(this ILogger logger, string action, string operation, Action<MeasurementLogData> func, LogLevel level = LogLevel.Information, LogData logData = null)
    {
        logger.Measure(action, operation, d =>
        {
            func(d);
            return true;
        }, level, logData);
    }

    public static void Measure(this ILogger logger, string action, Action<MeasurementLogData> func, LogLevel level = LogLevel.Information, LogData logData = null)
    {
        logger.Measure(action, null, d =>
        {
            func(d);
            return true;
        }, level, logData);
    }

    public static async Task<T> MeasureAsync<T>(this ILogger logger, string action, string operation, Func<MeasurementLogData, Task<T>> func, LogLevel logLevel = LogLevel.Information, LogData logData = null)
    {
        var sw = new Stopwatch();
        sw.Start();

        var data = new MeasurementLogData(logLevel, sw).AddFields(logData).AddField("Measure", true);

        T result;
        try
        {
            result = await func(data);
        }
        catch (Exception e)
        {
            e.AddData(data);
            logger.Log(LogLevel.Error, e, e.Message);
            throw;
        }

        sw.Stop();

        if (!data.Omit)
        {
            if (string.IsNullOrEmpty(operation))
            {
                logger.Log(data.LogLevel, "{action} took {elapsed} ms. {details}", action, sw.Elapsed.TotalMilliseconds, data.GetData());
            }
            else
            {
                logger.Log(data.LogLevel, "Executed {action} for {operation} in {elapsed} ms. {details}", action, operation, sw.Elapsed.TotalMilliseconds, data.GetData());
            }
        }

        return result;
    }

    public static Task<T> MeasureAsync<T>(this ILogger logger, string action, Func<MeasurementLogData, Task<T>> func, LogLevel level = LogLevel.Information, LogData logData = null)
    {
        return logger.MeasureAsync(action, null, func, level, logData);
    }

    public static Task MeasureAsync(this ILogger logger, string action, Func<MeasurementLogData, Task> func, LogLevel level = LogLevel.Information, LogData logData = null)
    {
        return logger.MeasureAsync(action, null, async d =>
        {
            await func(d);
            return true;
        }, level, logData);
    }

    public static Task MeasureAsync(this ILogger logger, string action, string operation, Func<MeasurementLogData, Task> func, LogLevel level = LogLevel.Information, LogData logData = null)
    {
        return logger.MeasureAsync(action, operation, async d =>
        {
            await func(d);
            return true;
        }, level, logData);
    }
}