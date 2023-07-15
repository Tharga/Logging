using System;

namespace Tharga.Logging;

public static class ExceptionExtension
{
    public static T AddData<T>(this T item, object key, object value)
        where T : Exception
    {
        if (item.Data.Contains(key)) item.Data.Remove(key);
        item.Data.Add(key, value);
        return item;
    }

    public static T AddData<T>(this T item, LogData data)
        where T : Exception
    {
        foreach (var dataItem in data.GetData())
        {
            item.AddData(dataItem.Key, dataItem.Value);
        }
        return item;
    }
}