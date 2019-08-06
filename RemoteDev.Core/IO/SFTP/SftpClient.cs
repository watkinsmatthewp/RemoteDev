using RemoteDev.Core.Loggers;
using Renci.SshNet;
using System;
using System.IO;
using RenciSftpClient = Renci.SshNet.SftpClient;

namespace RemoteDev.Core.IO.SFTP
{
    public class SftpClient : FileInteractionClient<SftpClientConfiguration>
    {
        readonly Lazy<RenciSftpClient> _sftpClient;

        public SftpClient(SftpClientConfiguration options, IRemoteDevLogger logger) : base(options, logger)
        {
            _sftpClient = new Lazy<RenciSftpClient>(InstantiateClient);
        }

        public override void Delete(string relativePath)
        {
            _logger.Log(LogLevel.DEBUG, $"SFTP: Deleting {relativePath} on the remote");
            _sftpClient.Value.Delete(relativePath);
        }

        public override void Put(string relativePath, Stream file)
        {
            _logger.Log(LogLevel.DEBUG, $"SFTP: Uploading file {relativePath} to the remote");
            _sftpClient.Value.UploadFile(file, relativePath.Replace("\\", "/"));
        }

        #region Private helpers

        RenciSftpClient InstantiateClient()
        {
            var connectionInfo = new ConnectionInfo(Options.Host, Options.UserName, new PasswordAuthenticationMethod(Options.UserName, Options.Password));
            var client = new RenciSftpClient(connectionInfo);

            client.Connect();
            if (!client.ConnectionInfo.IsAuthenticated)
            {
                throw new Exception("SFTP: Could not authenticate");
            }

            if (!string.IsNullOrWhiteSpace(Options.RemoteWorkingDirectory))
            {
                client.ChangeDirectory(Options.RemoteWorkingDirectory);
                if (client.WorkingDirectory != Options.RemoteWorkingDirectory)
                {
                    throw new Exception("SFTP: Do not match");
                }
            }

            return client;
        }

        #endregion
    }
}
