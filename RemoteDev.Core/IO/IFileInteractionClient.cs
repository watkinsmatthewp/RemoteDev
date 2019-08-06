using System.IO;

namespace RemoteDev.Core.IO
{
    public interface IFileInteractionClient
    {
        void PutFile(string relativePath, Stream file);
        void CreateDirectory(string relativePath);
        void DeleteFile(string relativePath);
        void DeleteDirectory(string relativePath);
    }
}