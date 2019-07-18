using CommandLine;
using RemoteDev.Core;
using RemoteDev.Core.FilePathFilters;
using RemoteDev.Core.FileWatching;
using RemoteDev.Core.IO;
using RemoteDev.Core.IO.LocalDirectory;
using RemoteDev.Core.IO.SFTP;
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

            Run(syncDirectoryVerbOptions, new LocalDirectoryClient(options));
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

            Run(syncSftpVerbOptions, new SftpClient(options));
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

        static void Run(ProgramOptions programOptions, IFileInteractionClient target)
        {
            var watcher = new FileWatcher(new FileWatcherConfig
            {
                WorkingDirectory = programOptions.WorkingDirectory,
                Verbose = programOptions.Verbose,
                MillisecondDelay = programOptions.MillisecondsDelay,
                ExclusionFilters = ReadGitIgnoreExclusions(programOptions.WorkingDirectory).ToList()
            });

            // Start watching files
            new RemoteDevWorker(watcher, target).Start();

            Console.WriteLine("Monitoring. Press any key to stop.");
            Console.ReadLine();
        }

        static IEnumerable<IFilePathFilter> ReadGitIgnoreExclusions(string rootDirectoryPath)
        {
            // Always ignore .git folder
            yield return new GlobFilePathFilter(".git");
            yield return new GlobFilePathFilter(".git/**");

            // Ignore files from root gitignore
            var gitIgnorePath = Path.Combine(rootDirectoryPath, ".gitignore");
            if (File.Exists(gitIgnorePath))
            {
                using (var fs = File.OpenRead(gitIgnorePath))
                using (var reader = new StreamReader(fs))
                {
                    while (!reader.EndOfStream)
                    {
                        var filter = GitIgnoreParser.ParseLine(reader.ReadLine());
                        if (filter != null)
                        {
                            yield return filter;
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"WARNING: No .gitignore found in root directory.");
            }
        }
    }
}
