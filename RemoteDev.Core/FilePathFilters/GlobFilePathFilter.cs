using DotNet.Globbing;
using System;

namespace RemoteDev.Core.FilePathFilters
{
    public class GlobFilePathFilter : IFilePathFilter
    {
        Glob _glob;

        public bool MatchDirectoryOnly { get; set; }
        public bool MatchFileOnly { get; set; }

        public GlobFilePathFilter(string globText)
        {
            if (string.IsNullOrWhiteSpace(globText))
            {
                throw new ArgumentException($"No {nameof(globText)} supplied");
            }
            _glob = Glob.Parse(globText);
        }

        public bool IsMatch(string relativePath, bool isFile)
        {
            if (isFile && MatchDirectoryOnly) return false;
            if (!isFile && MatchFileOnly) return false;
            return _glob.IsMatch(relativePath);
        }
    }
}
