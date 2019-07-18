namespace RemoteDev.Core.FilePathFilters
{
    public interface IFilePathFilter
    {
        bool IsMatch(string relativePath, bool isFile);
    }
}
