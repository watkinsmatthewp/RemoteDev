using RemoteDev.Core.FilePathFilters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RemoteDev.Core
{
    public static class GitIgnoreParser
    {
        public static GlobFilePathFilter ParseLine(string line)
        {
            // Rules from https://git-scm.com/docs/gitignore

            // A blank line matches no files, so it can serve as a separator for readability.
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            // A line starting with # serves as a comment.
            if (line.StartsWith("#"))
            {
                return null;
            }

            // Convert to glob pattern
            var globPattern = CreateGlobPattern(line);
            return new GlobFilePathFilter(globPattern);
        }

        #region Private helpers

        static string CreateGlobPattern(string line)
        {
            var pattern = line;

            // Trailing spaces are ignored unless they are quoted with backslash ("\").
            if (!pattern.EndsWith("\\ "))
            {
                pattern = pattern.TrimEnd();
            }

            // An optional prefix "!" which negates the pattern; any matching file excluded by a previous pattern will become included again. 
            if (pattern.StartsWith("!"))
            {
                throw new NotImplementedException("TODO: Negation operator currently not supported");
            }

            // If the pattern ends with a slash, ... it would only find a match with a directory.
            // In other words, foo/ will match a directory foo and paths underneath it
            if (pattern.EndsWith("/"))
            {
                pattern += "**";
            }
            else if (pattern.StartsWith("*"))
            {
                pattern = "**/" + pattern;
            }

            // Return the converted pattern
            return pattern;
        }

        #endregion
    }
}
