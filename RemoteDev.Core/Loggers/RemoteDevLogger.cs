using System;
using System.Text;

namespace RemoteDev.Core.Loggers
{
    public abstract class RemoteDevLogger : IRemoteDevLogger
    {
        protected RemoteDevLoggerConfig _configuration;

        protected RemoteDevLogger(RemoteDevLoggerConfig configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void Log(LogLevel logLevel, string message)
        {
            if (logLevel >= _configuration.MinimumLogLevel)
            {
                LogMessage(BuildMessage(logLevel, message));
            }
        }

        protected abstract void LogMessage(string message);

        string BuildMessage(LogLevel logLevel, string message)
        {
            var logLine = new StringBuilder();
            if (_configuration.PrintTimestamp)
            {
                logLine.Append(DateTime.Now).Append(" ");
            }
            if (_configuration.PrintLogLevel)
            {
                logLine.Append("[").Append(logLevel).Append("] ");
            }
            return logLine.Append(message).ToString();
        }
    }
}
