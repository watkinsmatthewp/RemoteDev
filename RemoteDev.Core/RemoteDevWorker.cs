using RemoteDev.Core.FileWatching;
using RemoteDev.Core.IO;
using RemoteDev.Core.Models;
using System;
using System.IO;

namespace RemoteDev.Core
{
    public class RemoteDevWorker
    {
        readonly IFileWatcher _fileWatcher;
        readonly IFileInteractionClient _fileClient;

        public RemoteDevWorker(IFileWatcher fileWatcher, IFileInteractionClient target)
        {
            _fileWatcher = fileWatcher ?? throw new ArgumentNullException(nameof(fileWatcher));
            _fileClient = target ?? throw new ArgumentNullException(nameof(target));
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
            _fileWatcher.Config.Log($"{fileChange.FileChangeType}: {relativePath}");

            switch (fileChange.FileChangeType)
            {
                case FileChangeType.Deleted:
                {
                    _fileClient.Delete(relativePath);
                    break;
                }
                default:
                {
                    var absolutePath = Path.Combine(_fileWatcher.Config.WorkingDirectory, relativePath);
                    if (File.Exists(absolutePath))
                    {
                        using (var fs = File.OpenRead(absolutePath))
                        {
                            _fileClient.Put(relativePath, fs);
                        }
                    }
                    break;
                }
            }
        }

        #endregion
    }
}