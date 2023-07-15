using System.Collections.Generic;

namespace Tharga.Logging.Logtail;

public interface ILoggingDefaultData
{
    ILoggingDefaultData AddData(string key, object value);
    IDictionary<string, object> GetData();
}