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
        readonly Timer _eventTimer;
        readonly ConcurrentQueue<FileChange> _eventQueue = new ConcurrentQueue<FileChange>();
        readonly IRemoteDevLogger _logger;

        public event EventHandler<FileChange> OnChange;
        public FileWatcherConfig Config { get; private set; }

        public FileWatcher(FileWatcherConfig config, IRemoteDevLogger logger)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Create the event delay timer
            _eventTimer = new Timer(Config.MillisecondDelay) { AutoReset = false };
            _eventTimer.Elapsed += OnTimerTick;

            // Create the framework-provided file system watcher
            _frameworkFileWatcher = new FileSystemWatcher(Config.WorkingDirectory)
            {
                IncludeSubdirectories = Config.Recursive
            };

            _frameworkFileWatcher.Changed += HandleNonRenameChange;
            _frameworkFileWatcher.Created += HandleNonRenameChange;
            _frameworkFileWatcher.Deleted += HandleNonRenameChange;
            _frameworkFileWatcher.Renamed += HandleRename;
        }

        public void Start()
        {
            _eventTimer.Start();
            _frameworkFileWatcher.EnableRaisingEvents = true;
        }

        #region Private helpers

        void HandleNonRenameChange(object sender, FileSystemEventArgs e)
        {
            var relativePath = GetRelativePath(e.FullPath);
            _logger.Log(LogLevel.DEBUG, $"Received FS {e.ChangeType} event for {relativePath}");

            if (!ShouldIgnore(relativePath, false))
            {
                _eventQueue.Enqueue(CreateFileChange(ConvertFileChangeType(e.ChangeType), relativePath));
            }
        }

        void HandleRename(object sender, RenamedEventArgs e)
        {
            var oldRelativePath = GetRelativePath(e.OldFullPath);
            var newRelativePath = GetRelativePath(e.FullPath);
            _logger.Log(LogLevel.DEBUG, $"Received FS rename event {oldRelativePath} => newRelativePath");

            if (!ShouldIgnore(oldRelativePath, true))
            {
                _eventQueue.Enqueue(CreateFileChange(FileChangeType.Deleted, oldRelativePath));
            }
            if (!ShouldIgnore(newRelativePath, true))
            {
                _eventQueue.Enqueue(CreateFileChange(FileChangeType.Created, newRelativePath));
            }
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

        static FileChangeType ConvertFileChangeType(WatcherChangeTypes frameworkChangeType)
        {
            switch (frameworkChangeType)
            {
                case WatcherChangeTypes.Created: return FileChangeType.Created;
                case WatcherChangeTypes.Deleted: return FileChangeType.Deleted;
                case WatcherChangeTypes.Changed: return FileChangeType.Modified;
                default: throw new ArgumentException($"Cannot convert {frameworkChangeType} to exactly one {nameof(FileChangeType)}");
            }
        }

        static FileChange CreateFileChange(FileChangeType fileChangeType, string relativePath) => new FileChange
        {
            FileChangeType = fileChangeType,
            RelativePathComponents = relativePath.Split(Path.DirectorySeparatorChar),
            FileEntityType = FileEntityType.File
        };

        void OnTimerTick(object sender, ElapsedEventArgs e)
        {
            _logger.Log(LogLevel.TRACE, "Timer ticked");
            var processedEventHashCodes = new HashSet<int>();
            while (!_eventQueue.IsEmpty)
            {
                if (_eventQueue.TryDequeue(out FileChange fileChangeEvent))
                {
                    var hashCode = fileChangeEvent.GetHashCode();
                    _logger.Log(LogLevel.TRACE, $"Popped file change event {hashCode} from queue");
                    if (processedEventHashCodes.Add(hashCode))
                    {
                        _logger.Log(LogLevel.TRACE, $"{hashCode} is a new event. Invoking {nameof(OnChange)}");
                        OnChange?.Invoke(this, fileChangeEvent);
                    }
                }
            }

            _logger.Log(LogLevel.TRACE, "Restarting timer");
            _eventTimer.Start();
        }

        #endregion
    }
}
