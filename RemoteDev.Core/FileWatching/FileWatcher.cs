using RemoteDev.Core.Loggers;
using RemoteDev.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace RemoteDev.Core.FileWatching
{
    public class FileWatcher : IFileWatcher
    {
        readonly FileSystemWatcher _frameworkFileWatcher;
        readonly FileSystemWatcher _frameworkDirectoryWatcher;

        readonly Timer _eventTimer;
        readonly ConcurrentQueue<FileSystemChange> _fileEventQueue = new ConcurrentQueue<FileSystemChange>();
        readonly ConcurrentQueue<FileSystemChange> _directoryEventQueue = new ConcurrentQueue<FileSystemChange>();
        readonly IRemoteDevLogger _logger;

        public event EventHandler<FileSystemChange> OnChange;
        public FileWatcherConfig Config { get; private set; }

        public FileWatcher(FileWatcherConfig config, IRemoteDevLogger logger)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Create the event delay timer
            _eventTimer = new Timer(Config.MillisecondDelay) { AutoReset = false };
            _eventTimer.Elapsed += OnTimerTick;

            // Create the framework-provided file watcher
            _frameworkFileWatcher = new FileSystemWatcher(Config.WorkingDirectory)
            {
                IncludeSubdirectories = Config.Recursive,
                NotifyFilter = NotifyFilters.FileName
            };

            _frameworkFileWatcher.Changed += HandleNonRenameChange;
            _frameworkFileWatcher.Created += HandleNonRenameChange;
            _frameworkFileWatcher.Deleted += HandleNonRenameChange;
            _frameworkFileWatcher.Renamed += HandleRename;

            // Create the framework-provided directory wather
            _frameworkDirectoryWatcher = new FileSystemWatcher(Config.WorkingDirectory)
            {
                IncludeSubdirectories = Config.Recursive,
                NotifyFilter = NotifyFilters.DirectoryName
            };

            _frameworkDirectoryWatcher.Changed += HandleNonRenameChange;
            _frameworkDirectoryWatcher.Created += HandleNonRenameChange;
            _frameworkDirectoryWatcher.Deleted += HandleNonRenameChange;
            _frameworkDirectoryWatcher.Renamed += HandleRename;
        }

        public void Start()
        {
            _eventTimer.Start();
            _frameworkFileWatcher.EnableRaisingEvents = true;
            _frameworkDirectoryWatcher.EnableRaisingEvents = true;
        }

        #region Private helpers

        void HandleNonRenameChange(object sender, FileSystemEventArgs e)
        {
            var relativePath = GetRelativePath(e.FullPath);
            _logger.Log(LogLevel.DEBUG, $"Received FS {e.ChangeType} event for {relativePath}");
            
            if (!ShouldIgnore(relativePath, false))
            {
                QueueFileSystemChangeEvent(CreateSystemFileChange(sender, ConvertFileSystemChangeType(e.ChangeType), relativePath, e.FullPath));
            }
        }

        void HandleRename(object sender, RenamedEventArgs e)
        {
            var oldRelativePath = GetRelativePath(e.OldFullPath);
            var newRelativePath = GetRelativePath(e.FullPath);
            _logger.Log(LogLevel.DEBUG, $"Received FS rename event {oldRelativePath} => {newRelativePath}");

            // Send the delete/create events that will represent the rename
            if (!ShouldIgnore(oldRelativePath, true))
            {
                QueueFileSystemChangeEvent(CreateSystemFileChange(sender, FileSystemChangeType.Deleted, oldRelativePath, e.OldFullPath));
            }
            if (!ShouldIgnore(oldRelativePath, true))
            {
                QueueFileSystemChangeEvent(CreateSystemFileChange(sender, FileSystemChangeType.Created, newRelativePath, e.OldFullPath));
            }
        }

        void QueueFileSystemChangeEvent(FileSystemChange fileSystemChange)
        {
            var queue = fileSystemChange.FileSystemEntityType == FileSystemEntityType.Directory ? _directoryEventQueue : _fileEventQueue;
            queue.Enqueue(fileSystemChange);
        }

        bool ShouldIgnore(string relativePath, bool isFile)
        {
            if (Config.ExclusionFilters?.Any(f => f.IsMatch(relativePath, isFile)) == true)
            {
                _logger.Log(LogLevel.INFO, $"Ignoring file change for {relativePath}");
                return true;
            }

            return false;
        }

        string GetRelativePath(string fullPath) => fullPath.Substring(Config.WorkingDirectory.Length + 1);

        static FileSystemChangeType ConvertFileSystemChangeType(WatcherChangeTypes frameworkChangeType)
        {
            switch (frameworkChangeType)
            {
                case WatcherChangeTypes.Created: return FileSystemChangeType.Created;
                case WatcherChangeTypes.Deleted: return FileSystemChangeType.Deleted;
                case WatcherChangeTypes.Changed: return FileSystemChangeType.Modified;
                default: throw new ArgumentException($"Cannot convert {frameworkChangeType} to exactly one {nameof(FileSystemChangeType)}");
            }
        }

        FileSystemChange CreateSystemFileChange(object sender, FileSystemChangeType fileChangeType, string relativePath, string absolutePath) => new FileSystemChange
        {
            FileSystemChangeType = fileChangeType,
            RelativePathComponents = relativePath.Split(Path.DirectorySeparatorChar),
            FileSystemEntityType = sender == _frameworkDirectoryWatcher ? FileSystemEntityType.Directory : FileSystemEntityType.File
        };

        void OnTimerTick(object sender, ElapsedEventArgs e)
        {
            _logger.Log(LogLevel.TRACE, "Timer ticked");
            var processedEventHashCodes = new HashSet<int>();

            while (!_fileEventQueue.IsEmpty || _directoryEventQueue.IsEmpty)
            {
                if (_fileEventQueue.TryDequeue(out FileSystemChange fileChangeEvent))
                {
                    PromoteFileSystemChange(processedEventHashCodes, fileChangeEvent);
                }
                else if (_directoryEventQueue.TryDequeue(out FileSystemChange directoryChangeEvent))
                {
                    // Only run directory action if there was nothing in the file queue
                    PromoteFileSystemChange(processedEventHashCodes, directoryChangeEvent);
                }
            }

            _logger.Log(LogLevel.TRACE, "Restarting timer");
            _eventTimer.Start();
        }

        private void PromoteFileSystemChange(HashSet<int> processedEventHashCodes, FileSystemChange fileSystemChangeEvent)
        {
            var hashCode = fileSystemChangeEvent.GetHashCode();
            _logger.Log(LogLevel.TRACE, $"Popped {fileSystemChangeEvent.FileSystemEntityType} {fileSystemChangeEvent.FileSystemChangeType} event {hashCode} from queue");
            if (processedEventHashCodes.Add(hashCode))
            {
                _logger.Log(LogLevel.TRACE, $"{hashCode} is a new event. Invoking {nameof(OnChange)}");
                OnChange?.Invoke(this, fileSystemChangeEvent);
            }
        }

        #endregion
    }
}
