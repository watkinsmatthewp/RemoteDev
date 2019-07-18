using CommandLine;

namespace RemoteDev.Options
{
    public class ProgramOptions
    {
        [Option('l', "local", Required = true, HelpText = "The local directory to sync against")]
        public string WorkingDirectory { get; set; }

        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.", Default = false)]
        public bool Verbose { get; set; }

        [Option('d', "delay", Required = false, HelpText = "The number of milliseconds to wait before processing a file change (avoids duplicates, default 300)", Default = 300)]
        public int MillisecondsDelay { get; set; }
    }
}