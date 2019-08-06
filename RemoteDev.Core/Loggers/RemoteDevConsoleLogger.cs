namespace RemoteDev.Core.Loggers
{
    public class RemoteDevConsoleLogger : RemoteDevLogger
    {
        public RemoteDevConsoleLogger(RemoteDevLoggerConfig configuration)
            : base(configuration)
        {
        }

        protected override void LogMessage(string message) => System.Console.WriteLine(message);
    }
}
