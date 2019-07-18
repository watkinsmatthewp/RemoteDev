using System.IO;

namespace RemoteDev.Core.IO
{
    public interface IFileInteractionClient
    {
        void Put(string relativePath, Stream file);
        void Delete(string relativePath);
    }
}