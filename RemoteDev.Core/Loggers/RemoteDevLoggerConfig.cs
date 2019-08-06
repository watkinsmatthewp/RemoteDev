namespace RemoteDev.Core.Loggers
{
    public class RemoteDevLoggerConfig
    {
        public LogLevel MinimumLogLevel { get; set; }
        public bool PrintLogLevel { get; set; } = true;
        public bool PrintTimestamp { get; set; } = true;
    }
}
