namespace Tharga.Logging.Logtail;

public class SendLogEventArgs
{
    public SendLogEventArgs(LogEntry logEntry)
    {
        LogEntry = logEntry;
    }

    public LogEntry LogEntry { get; }
}