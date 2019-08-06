namespace RemoteDev.Core.Loggers
{
    public interface IRemoteDevLogger
    {
        void Log(LogLevel logLevel, string message);
    }
}
