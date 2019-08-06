using RemoteDev.Core.Loggers;
using System;
using System.IO;

namespace RemoteDev.Core.IO.LocalDirectory
{
    public class LocalDirectoryClient : FileInteractionClient<LocalDirectoryClientConfiguration>
    {        
        public LocalDirectoryClient(LocalDirectoryClientConfiguration options, IRemoteDevLogger logger) : base(options, logger)
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
                _logger.Log(LogLevel.DEBUG, $"DIR: Deleting file {relativePath} on the remote path");
                File.Delete(CreateAbsolutePath(relativePath));
            }
            catch (FileNotFoundException)
            {
                _logger.Log(LogLevel.WARN, $"DIR: Could not find file {relativePath} remote path. Could not delete");
            }
        }

        public override void Put(string relativePath, Stream file)
        {
            _logger.Log(LogLevel.DEBUG, $"DIR: Copying file {relativePath} to the remote path");
            using (file)
            using (var fs = File.Create(CreateAbsolutePath(relativePath)))
            {
                file.CopyTo(fs);
            }
        }

        string CreateAbsolutePath(string relativePath) => Path.Join(Options.Path, relativePath);
    }
}