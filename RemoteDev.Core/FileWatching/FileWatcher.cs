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
        FileSystemWatcher _frameworkFileWatcher;
        Timer _eventTimer;
        ConcurrentQueue<FileChange> _eventQueue = new ConcurrentQueue<FileChange>();

        public event EventHandler<FileChange> OnChange;
        public FileWatcherConfig Config { get; private set; }

        public FileWatcher(FileWatcherConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));

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
            if (!ShouldIgnore(relativePath, false))
            {
                _eventQueue.Enqueue(CreateFileChange(ConvertFileChangeType(e.ChangeType), relativePath));
            }
        }

        void HandleRename(object sender, RenamedEventArgs e)
        {
            var oldRelativePath = GetRelativePath(e.OldFullPath);
            if (!ShouldIgnore(oldRelativePath, false))
            {
                _eventQueue.Enqueue(CreateFileChange(FileChangeType.Deleted, oldRelativePath));
            }

            var newRelativePath = GetRelativePath(e.FullPath);
            if (!ShouldIgnore(newRelativePath, false))
            {
                _eventQueue.Enqueue(CreateFileChange(FileChangeType.Created, newRelativePath));
            }
        }

        bool ShouldIgnore(string relativePath, bool isFile)
        {
            if (Config.ExclusionFilters?.Any(f => f.IsMatch(relativePath, isFile)) == true)
            {
                Config.Log($"Ignoring file change for {relativePath}");
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
            var processedEventHashCodes = new HashSet<int>();
            while (!_eventQueue.IsEmpty)
            {
                if (_eventQueue.TryDequeue(out FileChange fileChangeEvent) && processedEventHashCodes.Add(fileChangeEvent.GetHashCode()))
                {
                    OnChange?.Invoke(this, fileChangeEvent);
                }
            }

            _eventTimer.Start();
        }

        #endregion
    }
}
