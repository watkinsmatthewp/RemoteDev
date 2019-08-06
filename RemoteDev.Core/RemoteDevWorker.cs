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

        void HandleOnChangeEvent(object sender, FileChange fileChange)
        {
            var relativePath = fileChange.GetRelativePath();
            _logger.Log(LogLevel.DEBUG, $"Worker received {fileChange.FileChangeType} event for {relativePath}");

            switch (fileChange.FileChangeType)
            {
                case FileChangeType.Deleted:
                {
                    _logger.Log(LogLevel.DEBUG, "About to delete " + relativePath);
                    _fileClient.Delete(relativePath);
                    break;
                }
                default:
                {
                    var absolutePath = Path.Combine(_fileWatcher.Config.WorkingDirectory, relativePath);
                    if (File.Exists(absolutePath))
                    {
                        _logger.Log(LogLevel.DEBUG, "About to put " + relativePath);
                        using (var fs = File.OpenRead(absolutePath))
                        {
                            _fileClient.Put(relativePath, fs);
                        }
                    }
                    else
                    {
                        _logger.Log(LogLevel.WARN, "Cannot put file as it does not exist: " + relativePath);
                    }
                    break;
                }
            }
        }

        #endregion
    }
}