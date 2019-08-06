using RemoteDev.Core.Loggers;
using System;
using System.IO;

namespace RemoteDev.Core.IO
{
    public abstract class FileInteractionClient<O> : IFileInteractionClient where O : class
    {
        protected IRemoteDevLogger _logger { get; private set; }

        public O Options { get; private set; }     

        protected FileInteractionClient(O options, IRemoteDevLogger logger)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public abstract void PutFile(string relativePath, Stream file);
        public abstract void CreateDirectory(string relativePath);
        public abstract void DeleteFile(string relativePath);
        public abstract void DeleteDirectory(string relativePath);
    }
}