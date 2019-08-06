using RemoteDev.Core.FilePathFilters;
using System.Collections.Generic;

namespace RemoteDev.Core.FileWatching
{
    public class FileWatcherConfig
    {
        public string WorkingDirectory { get; set; }
        public bool Recursive { get; set; } = true;
        public List<IFilePathFilter> ExclusionFilters { get; set; } = new List<IFilePathFilter>();
        public int MillisecondDelay { get; set; } = 300;
    }
}
