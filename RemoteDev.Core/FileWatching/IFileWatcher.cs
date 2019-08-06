using RemoteDev.Core.Models;
using System;

namespace RemoteDev.Core.FileWatching
{
    public interface IFileWatcher
    {
        FileWatcherConfig Config { get; }
        event EventHandler<FileSystemChange> OnChange;
        void Start();
    }
}
