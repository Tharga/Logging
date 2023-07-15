using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tharga.Toolkit;

namespace Tharga.Logging;

public static class LogDataExtensions
{
    public static LogData ToLogData(this IEnumerable<KeyValuePair<string, object>> item)
    {
        var logData = new LogData();
        foreach (var data in item)
        {
            logData.AddField(data.Key, data.Value);
        }

        return logData;
    }

    public static Dictionary<string, object> GetData(this LogEntry logEntry)
    {
        var data = logEntry.Data?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, object>();

        if (logEntry.Exception != null)
        {
            data.TryAdd("stackTrace", logEntry.Exception.StackTrace);
            data.TryAdd("stackTraceHash", logEntry.Exception.StackTrace?.ToHash(HashExtensions.Style.Base64));

            foreach (DictionaryEntry entry in logEntry.Exception.Data)
            {
                data.TryAdd(entry.Key.ToString(), entry.Value);
            }
        }

        data.TryAdd("{OriginalFormat}", logEntry.Message);

        return data;
    }

    public static Dictionary<string, object> GetData(this LogData logData)
    {
        var data = logData?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, object>();
        return data;
    }

    public static TLogData AddField<TLogData, T>(this TLogData logData, string key, T data)
        where TLogData : LogData
    {
        logData.AddData(key, data);
        return logData;
    }

    public static TLogData AddFields<TLogData>(this TLogData logData, LogData data)
        where TLogData : LogData
    {
        if (data == null) return logData;

        foreach (var item in data)
        {
            logData.AddData(item.Key, item.Value);
        }
        return logData;
    }
}