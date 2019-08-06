using CommandLine;
using RemoteDev.Core.Loggers;

namespace RemoteDev.Options
{
    public class ProgramOptions
    {
        [Option('l', "local", Required = true, HelpText = "The local directory to sync against")]
        public string WorkingDirectory { get; set; }

        [Option('v', "verbosity", Required = false, HelpText = "The verbosity level of the output. Allowed values: FATAL, ERROR, WARN, INFO, DEBUG, TRACE", Default = LogLevel.WARN)]
        public LogLevel Verbosity { get; set; }

        [Option('d', "delay", Required = false, HelpText = "The number of milliseconds to wait before processing a file change (avoids duplicates, default 300)", Default = 300)]
        public int MillisecondsDelay { get; set; }
    }
}