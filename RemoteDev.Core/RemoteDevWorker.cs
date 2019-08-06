using RemoteDev.Core.FileWatching;
using RemoteDev.Core.IO;
using RemoteDev.Core.Loggers;
using RemoteDev.Core.Models;
using System;
using System.IO;

namespace RemoteDev.Core
{
    public class RemoteDevWorker
    {
        readonly IFileWatcher _fileWatcher;
        readonly IFileInteractionClient _fileClient;
        readonly IRemoteDevLogger _logger;

        public RemoteDevWorker(IFileWatcher fileWatcher, IFileInteractionClient target, IRemoteDevLogger logger)
        {
            _fileWatcher = fileWatcher ?? throw new ArgumentNullException(nameof(fileWatcher));
            _fileClient = target ?? throw new ArgumentNullException(nameof(target));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Start()
        {
            _fileWatcher.OnChange += HandleOnChangeEvent;
            _fileWatcher.Start();
        }

        #region Private helpers

        void HandleOnChangeEvent(object sender, FileSystemChange fileSystemChange)
        {
            var relativePath = fileSystemChange.GetRelativePath();
            _logger.Log(LogLevel.DEBUG, $"Worker received {fileSystemChange.FileSystemEntityType} {fileSystemChange.FileSystemChangeType} event for {relativePath}");

            if (fileSystemChange.FileSystemEntityType == FileSystemEntityType.File)
            {
                if (fileSystemChange.FileSystemChangeType == FileSystemChangeType.Deleted)
                {
                    DeleteFile(relativePath);
                }
                else
                {
                    PutFile(relativePath);
                }
            }
            else if (fileSystemChange.FileSystemEntityType == FileSystemEntityType.Directory)
            {
                if (fileSystemChange.FileSystemChangeType == FileSystemChangeType.Deleted)
                {
                    DeleteDirectory(relativePath);
                }
                else if (fileSystemChange.FileSystemChangeType == FileSystemChangeType.Created)
                {
                    CreateDirectory(relativePath);
                }
                else
                {
                    _logger.Log(LogLevel.DEBUG, $"No need to process {fileSystemChange.FileSystemEntityType} {fileSystemChange.FileSystemChangeType} for {relativePath}");
                }
            }
            else
            {
                if (fileSystemChange.FileSystemChangeType == FileSystemChangeType.Deleted)
                {
                    DeleteFile(relativePath);
                    DeleteDirectory(relativePath);
                }
                else
                {
                    _logger.Log(LogLevel.WARN, $"Cannot to process {fileSystemChange.FileSystemEntityType} {fileSystemChange.FileSystemChangeType} for {relativePath}");
                }
            }
        }

        void CreateDirectory(string relativePath)
        {
            _logger.Log(LogLevel.DEBUG, "About to create directory " + relativePath);
            _fileClient.CreateDirectory(relativePath);
        }

        void DeleteDirectory(string relativePath)
        {
            _logger.Log(LogLevel.DEBUG, "About to try delete directory " + relativePath);
            _fileClient.DeleteDirectory(relativePath);
        }

        void PutFile(string relativePath)
        {
            _logger.Log(LogLevel.DEBUG, "About to put file " + relativePath);
            var absolutePath = Path.Combine(_fileWatcher.Config.WorkingDirectory, relativePath);
            try
            {
                using (var fs = File.OpenRead(absolutePath))
                {
                    _fileClient.PutFile(relativePath, fs);
                }
            }
            catch (FileNotFoundException)
            {
                _logger.Log(LogLevel.WARN, $"Cannot put file {relativePath} since it does not exist.");
            }
        }

        void DeleteFile(string relativePath)
        {
            _logger.Log(LogLevel.DEBUG, "About to try delete file " + relativePath);
            _fileClient.DeleteFile(relativePath);
        }

        #endregion
    }
}