namespace RemoteDev.Core.IO.SFTP
{
    public class SftpClientConfiguration
    {
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string RemoteWorkingDirectory { get; set; }
    }
}
