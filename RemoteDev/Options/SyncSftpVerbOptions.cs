using CommandLine;

namespace RemoteDev.Options
{
    [Verb("sync-sftp", HelpText = "Sync contents with an SFTP directory")]
    public class SyncSftpVerbOptions : ProgramOptions
    {
        [Option('h', "host", Required = true, HelpText = "The host to sync to")]
        public string Host { get; set; }

        [Option('w', "working-directory", Required = false, HelpText = "The working directory on the remote to use")]
        public string RemoteWorkingDirectory { get; set; }

        [Option('u', "user", Required = true, HelpText = "The username to use in the connection")]
        public string UserName { get; set; }

        [Option('p', "password", Required = false, HelpText = "The password to use in the connection")]
        public string Password { get; set; }
    }
}