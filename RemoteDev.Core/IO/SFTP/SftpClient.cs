using RemoteDev.Core.Loggers;
using Renci.SshNet;
using Renci.SshNet.Common;
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

        public override void DeleteFile(string relativePath)
        {
            relativePath = relativePath.Replace("\\", "/");
            _logger.Log(LogLevel.DEBUG, $"SFTP: Deleting file {relativePath} on the remote");
            try
            {
                _sftpClient.Value.DeleteFile(relativePath);
            }
            catch (SftpPathNotFoundException)
            {
                _logger.Log(LogLevel.WARN, $"SFTP: Cannot delete {relativePath} on the remote. It does not exist.");
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.ERROR, e.ToString());
            }
        }

        public override void DeleteDirectory(string relativePath)
        {
            relativePath = relativePath.Replace("\\", "/");
            _logger.Log(LogLevel.DEBUG, $"SFTP: Deleting directory {relativePath} on the remote");
            try
            {
                _sftpClient.Value.DeleteDirectory(relativePath);
            }
            catch (SftpPathNotFoundException)
            {
                _logger.Log(LogLevel.WARN, $"SFTP: Cannot delete {relativePath} on the remote. It does not exist.");
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.ERROR, e.ToString());
            }
        }

        public override void PutFile(string relativePath, Stream file)
        {
            relativePath = relativePath.Replace("\\", "/");
            _logger.Log(LogLevel.DEBUG, $"SFTP: Uploading file {relativePath} to the remote");
            _sftpClient.Value.UploadFile(file, relativePath);
        }

        public override void CreateDirectory(string relativePath)
        {
            relativePath = relativePath.Replace("\\", "/");
            _logger.Log(LogLevel.DEBUG, $"SFTP: Creating directory {relativePath} on the remote");
            _sftpClient.Value.CreateDirectory(relativePath);
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
