using CommandLine;

namespace RemoteDev.Options
{
    [Verb("sync-dir", HelpText = "Sync contents with another directory")]
    public class SyncDirectoryVerbOptions : ProgramOptions
    {
        [Option('r', "remote", Required = true, HelpText = "The target directory to sync to")]
        public string Remote { get; set; }
    }
}