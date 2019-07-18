using System.IO;

namespace RemoteDev.Core.Models
{
    public class FileChange
    {
        public FileEntityType FileEntityType { get; set; }
        public FileChangeType FileChangeType { get; set; }
        public string[] RelativePathComponents { get; set; }

        public string GetRelativePath() => Path.Combine(RelativePathComponents);
        public string GetRelativePath(char directorySeparatorChar) => string.Join(directorySeparatorChar, RelativePathComponents);

        public override int GetHashCode() => (string.Join("", RelativePathComponents) + FileChangeType + FileEntityType).GetHashCode();
    }
}
