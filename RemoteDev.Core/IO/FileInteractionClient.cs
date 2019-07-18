using System;
using System.IO;

namespace RemoteDev.Core.IO
{
    public abstract class FileInteractionClient<O> : IFileInteractionClient where O : class
    {
        public O Options { get; private set; }        

        protected FileInteractionClient(O options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public abstract void Put(string relativePath, Stream file);
        public abstract void Delete(string relativePath);
    }
}