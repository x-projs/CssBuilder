using LibSassHost;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SassBuilder
{
    class Options
    {
        public bool Recursive { get; set; } = false;
        public List<string> Srcs { get; } = new List<string>();

        /// <summary>
        /// Excludes files/directories.
        /// Null means use .gitignore. Empty means no excludes.
        /// </summary>
        public List<string> Excludes { get; set; }
    }

    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var options = ParseOptions(args);
                if (options.Srcs.Count == 0)
                {
                    PrintHelp();
                }
                else
                {
                    DetectRuntime();
                    Process(options);
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        static Options ParseOptions(string[] args)
        {
            var options = new Options();
            foreach (var str in args)
            {
                if (str[0] == '-')
                {
                    if (str == "-r" || str == "--recursive")
                    {
                        options.Recursive = true;
                    }
                    else
                    {
                        throw new ArgumentException($"Unknown options: {str}");
                    }
                }
                else
                {
                    options.Srcs.Add(Path.GetFullPath(str));
                }
            }
            return options;
        }

        static void DetectRuntime()
        {
            string runtimePath = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                switch (RuntimeInformation.OSArchitecture)
                {
                    case Architecture.X86:
                        runtimePath = "win-x86";
                        break;

                    case Architecture.X64:
                        runtimePath = "win-x64";
                        break;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                switch (RuntimeInformation.OSArchitecture)
                {
                    case Architecture.X64:
                        runtimePath = "linux-x64";
                        break;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                switch (RuntimeInformation.OSArchitecture)
                {
                    case Architecture.X64:
                        runtimePath = "osx-x64";
                        break;
                }
            }

            if (String.IsNullOrEmpty(runtimePath))
            {
                throw new PlatformNotSupportedException("Current platform is not suppored.");
            }

            runtimePath = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), "runtimes", runtimePath, "native");
            Environment.SetEnvironmentVariable("PATH", $"{runtimePath};{Environment.GetEnvironmentVariable("PATH")}");
        }

        static void PrintHelp()
        {
            Console.WriteLine("Usage: [-r/--resursive] src1 src2 ...");
        }

        static void Process(Options options)
        {
            foreach (var src in options.Srcs)
            {
                if (File.Exists(src))
                {
                    ProcessFile(src);
                }
                else if (Directory.Exists(src))
                {
                    ProcessDirectory(src, options.Recursive, options.Excludes == null ? GetExcludesFromGitIgnore(src) : options.Excludes);
                }
                else
                {
                    throw new FileNotFoundException($"Can't find file ${src}");
                }
            }
        }

        static void ProcessFile(string file)
        {
            var result = SassCompiler.CompileFile(file);
            File.WriteAllText(Path.ChangeExtension(file, ".css"), result.CompiledContent);
        }

        static void ProcessDirectory(string directory, bool recursive, IEnumerable<string> excludes)
        {
            if (excludes.Contains(directory + "/"))
            {
                return;
            }

            foreach (var file in Directory.GetFiles(directory, "*.scss", SearchOption.TopDirectoryOnly))
            {
                if (!excludes.Contains(file))
                {
                    ProcessFile(file);
                }
            }

            if (recursive)
            {
                foreach (var dir in Directory.GetDirectories(directory))
                {
                    ProcessDirectory(dir, recursive, excludes);
                }
            }
        }

        static IEnumerable<string> GetExcludesFromGitIgnore(string dir)
        {
            try
            {
                LogDebug($"Get git ignore from {dir}");
                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = "git",
                        Arguments = "ls-files --others --ignored --exclude-standard --directory",
                        RedirectStandardOutput = true,
                        WorkingDirectory = dir,
                    }
                };
                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                LogDebug($"{process.StartInfo.FileName} {process.StartInfo.Arguments}:\n{output}");
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    LogDebug($"GetExcludesFromGitIgnore failed, exit code: {process.ExitCode}");
                    return Enumerable.Empty<string>();
                }
                return output.Split("\n", StringSplitOptions.RemoveEmptyEntries).Select(s =>
                {
                    var fullPath = Path.Combine(dir, s);
                    LogDebug($"Add exclude path '{fullPath}' from gitignore");
                    return fullPath;
                }).ToArray();
            }
            catch (Exception ex)
            {
                LogDebug($"GetExcludesFromGitIgnore exception: {ex.Message}\n{ex.StackTrace}");

                // Ignore all errors.
                return Enumerable.Empty<string>();
            }
        }

        static void LogDebug(string str)
        {
#if DEBUG
            Console.WriteLine("[SassBuilder]: " + str);
#endif
        }
    }
}
