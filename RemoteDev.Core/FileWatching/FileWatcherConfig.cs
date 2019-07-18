using RemoteDev.Core.FilePathFilters;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RemoteDev.Core.FileWatching
{
    public class FileWatcherConfig
    {
        bool _verbose;
        Action<string> _logAction;

        public string WorkingDirectory { get; set; }
        public bool Recursive { get; set; } = true;
        public List<IFilePathFilter> ExclusionFilters { get; set; } = new List<IFilePathFilter>();
        public int MillisecondDelay { get; set; } = 300;

        public bool Verbose
        {
            get => _verbose;
            set
            {
                if (_verbose = value)
                {
                    _logAction = Console.WriteLine;
                }
                else
                {
                    _logAction = l => Debug.WriteLine(l);
                }
            }
        }

        public void Log(string s) => _logAction.Invoke(s);
    }
}
