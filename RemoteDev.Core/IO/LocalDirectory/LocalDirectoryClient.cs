using System;
using System.IO;

namespace RemoteDev.Core.IO.LocalDirectory
{
    public class LocalDirectoryClient : FileInteractionClient<LocalDirectoryClientConfiguration>
    {        
        public LocalDirectoryClient(LocalDirectoryClientConfiguration options) : base(options)
        {
            if (string.IsNullOrWhiteSpace(options.Path))
            {
                throw new ArgumentException("No path specified");
            }
        }

        public override void Delete(string relativePath)
        {
            try
            {
                File.Delete(CreateAbsolutePath(relativePath));
            }
            catch (FileNotFoundException) { }
        }

        public override void Put(string relativePath, Stream file)
        {
            using (file)
            using (var fs = File.Create(CreateAbsolutePath(relativePath)))
            {
                file.CopyTo(fs);
            }
        }

        string CreateAbsolutePath(string relativePath) => Path.Join(Options.Path, relativePath);
    }
}