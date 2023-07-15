using System.Collections.Generic;
using System.Linq;

namespace Tharga.Logging.Logtail;

public static class MessageBuilder
{
    public static string AppendLogData(this string message, IDictionary<string, object> data)
    {
        if (message?.Contains("[") ?? true) return message;

        if (data?.Any() ?? false)
        {
            var sd = string.Join(", ", data.Select(x => $"{x.Key}: {x.Value}"));
            return $"{message} [{sd}]";
        }

        return message;
    }
}