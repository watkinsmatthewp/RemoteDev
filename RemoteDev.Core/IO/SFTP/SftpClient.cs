using RemoteDev.Core.Loggers;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.IO;
using RenciSftpClient = Renci.SshNet.SftpClient;
using RenciSshClient = Renci.SshNet.SshClient;

namespace RemoteDev.Core.IO.SFTP
{
    public class SftpClient : FileInteractionClient<SftpClientConfiguration>
    {
        readonly Lazy<RenciSftpClient> _sftpClient;
        readonly Lazy<RenciSshClient> _sshClient;

        public SftpClient(SftpClientConfiguration options, IRemoteDevLogger logger) : base(options, logger)
        {
            _sftpClient = new Lazy<RenciSftpClient>(InstantiateSftpClient);
            _sshClient = new Lazy<RenciSshClient>(InstantiateSshClient);
        }

        public override void DeleteFile(string relativePath)
        {
            var absolutePath = CreateAbsolutePath(relativePath);
            _logger.Log(LogLevel.DEBUG, $"SFTP: Deleting file {absolutePath} on the remote");
            try
            {
                lock (this)
                {
                    _sftpClient.Value.DeleteFile(absolutePath); 
                }
                _logger.Log(LogLevel.DEBUG, $"SFTP: Deleted");
            }
            catch (SftpPathNotFoundException)
            {
                _logger.Log(LogLevel.WARN, $"SFTP: Cannot delete {absolutePath} on the remote. It does not exist.");
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.ERROR, e.ToString());
            }
        }

        public override void DeleteDirectory(string relativePath)
        {
            var absolutePath = CreateAbsolutePath(relativePath);
            _logger.Log(LogLevel.DEBUG, $"SFTP: Deleting directory {absolutePath} on the remote");
            try
            {
                lock (this)
                {
                    _sftpClient.Value.DeleteDirectory(absolutePath);
                }
                _logger.Log(LogLevel.DEBUG, $"SFTP: Deleted");
            }
            catch (SftpPathNotFoundException)
            {
                _logger.Log(LogLevel.WARN, $"SFTP: Cannot delete {absolutePath} on the remote. It does not exist.");
            }
            catch (Exception)
            {
                _logger.Log(LogLevel.WARN, $"An error occurred. Perhaps the directory wasn't empty? Re-trying with -fr");
                try
                {
                    _sshClient.Value.RunCommand($"rm -fr {absolutePath}");
                    _logger.Log(LogLevel.DEBUG, $"SFTP: Force-deleted");
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.ERROR, e.ToString());
                }
            }
        }

        public override void PutFile(string relativePath, Stream file)
        {
            var absolutePath = CreateAbsolutePath(relativePath);
            _logger.Log(LogLevel.DEBUG, $"SFTP: Uploading file {absolutePath} to the remote");
            lock (this)
            {
                _sftpClient.Value.UploadFile(file, absolutePath);
            }
            _logger.Log(LogLevel.DEBUG, $"SFTP: Uploaded");
        }

        public override void CreateDirectory(string relativePath)
        {
            var absolutePath = CreateAbsolutePath(relativePath);
            _logger.Log(LogLevel.DEBUG, $"SFTP: Creating directory {absolutePath} on the remote");
            lock (this)
            {
                _sftpClient.Value.CreateDirectory(absolutePath);
            }
            _logger.Log(LogLevel.DEBUG, $"SFTP: Created");
        }

        #region Private helpers

        RenciSftpClient InstantiateSftpClient()
        {
            var connectionInfo = new ConnectionInfo(Options.Host, Options.UserName, new PasswordAuthenticationMethod(Options.UserName, Options.Password));
            var client = new RenciSftpClient(connectionInfo);

            client.Connect();
            if (!client.ConnectionInfo.IsAuthenticated)
            {
                throw new Exception("SFTP: Could not authenticate");
            }

            return client;
        }

        RenciSshClient InstantiateSshClient()
        {
            var client = new RenciSshClient(_sftpClient.Value.ConnectionInfo);
            client.Connect();
            return client;
        }

        string CreateAbsolutePath(string relativePath) => Options.RemoteWorkingDirectory + "/" + relativePath.Replace("\\", "/");

        #endregion
    }
}
