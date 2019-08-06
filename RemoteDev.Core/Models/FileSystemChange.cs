using System.IO;

namespace RemoteDev.Core.Models
{
    public class FileSystemChange
    {
        public FileSystemEntityType FileSystemEntityType { get; set; }
        public FileSystemChangeType FileSystemChangeType { get; set; }
        public string[] RelativePathComponents { get; set; }

        public string GetRelativePath() => Path.Combine(RelativePathComponents);
        public string GetRelativePath(char directorySeparatorChar) => string.Join(directorySeparatorChar, RelativePathComponents);

        public override int GetHashCode() => (string.Join("", RelativePathComponents) + FileSystemChangeType + FileSystemEntityType).GetHashCode();
    }
}
