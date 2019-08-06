using CommandLine;
using RemoteDev.Core;
using RemoteDev.Core.FilePathFilters;
using RemoteDev.Core.FileWatching;
using RemoteDev.Core.IO;
using RemoteDev.Core.IO.LocalDirectory;
using RemoteDev.Core.IO.SFTP;
using RemoteDev.Core.Loggers;
using RemoteDev.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RemoteDev
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
            Parser.Default.ParseArguments<SyncDirectoryVerbOptions, SyncSftpVerbOptions>(args)
                .WithParsed<SyncDirectoryVerbOptions>(RunDirectorySync)
                .WithParsed<SyncSftpVerbOptions>(RunSftpSync);
        }

        static void RunDirectorySync(SyncDirectoryVerbOptions syncDirectoryVerbOptions)
        {
            var options = new LocalDirectoryClientConfiguration
            {
                Path = syncDirectoryVerbOptions.Remote
            };

            var logger = BuildLogger(syncDirectoryVerbOptions);
            Run(syncDirectoryVerbOptions, new LocalDirectoryClient(options, logger), logger);
        }

        static void RunSftpSync(SyncSftpVerbOptions syncSftpVerbOptions)
        {
            var options = new SftpClientConfiguration
            {
                Host = syncSftpVerbOptions.Host,
                UserName = syncSftpVerbOptions.UserName,
                RemoteWorkingDirectory = syncSftpVerbOptions.RemoteWorkingDirectory,
                Password = syncSftpVerbOptions.Password
            };

            if (string.IsNullOrWhiteSpace(options.Password))
            {
                Console.WriteLine("Please enter your password:");
                options.Password = ReadPassword();
            }

            var logger = BuildLogger(syncSftpVerbOptions);
            Run(syncSftpVerbOptions, new SftpClient(options, logger), logger);
        }

        static string ReadPassword()
        {
            var sb = new StringBuilder();
            do
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    sb.Append(key.KeyChar);
                    // Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
                    {
                        sb.Remove(sb.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            } while (true);

            Console.WriteLine();
            return sb.ToString();
        }

        static IRemoteDevLogger BuildLogger(ProgramOptions programOptions) => new RemoteDevConsoleLogger(new RemoteDevLoggerConfig
        {
            MinimumLogLevel = programOptions.Verbosity
        });

        static void Run(ProgramOptions programOptions, IFileInteractionClient target, IRemoteDevLogger logger)
        {
            var watcher = new FileWatcher(new FileWatcherConfig
            {
                WorkingDirectory = programOptions.WorkingDirectory,
                MillisecondDelay = programOptions.MillisecondsDelay,
                ExclusionFilters = ReadGitIgnoreExclusions(programOptions.WorkingDirectory, logger).ToList()
            }, logger);

            // Start watching files
            Console.WriteLine("Starting file monitor");
            new RemoteDevWorker(watcher, target, logger).Start();

            Console.WriteLine("Monitoring. Press any key to stop.");
            Console.ReadLine();
        }

        static IEnumerable<IFilePathFilter> ReadGitIgnoreExclusions(string rootDirectoryPath, IRemoteDevLogger logger)
        {
            // Always ignore .git folder
            yield return new GlobFilePathFilter(".git");
            yield return new GlobFilePathFilter(".git/**");

            // Ignore files from root gitignore
            var gitIgnorePath = Path.Combine(rootDirectoryPath, ".gitignore");
            if (File.Exists(gitIgnorePath))
            {
                logger.Log(LogLevel.INFO, $"Using the gitignore found in {gitIgnorePath}");
                var filterCount = 0;

                using (var fs = File.OpenRead(gitIgnorePath))
                using (var reader = new StreamReader(fs))
                {
                    while (!reader.EndOfStream)
                    {
                        var filter = GitIgnoreParser.ParseLine(reader.ReadLine());
                        if (filter != null)
                        {
                            filterCount++;
                            yield return filter;
                        }
                    }
                }

                logger.Log(LogLevel.INFO, $"Found {filterCount} blob filters in the gitignore file");
            }
            else
            {
                logger.Log(LogLevel.WARN, "No .gitignore found in root directory. No file/folder changes will be ignored");
            }
        }
    }
}
