using LibSassHost;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace SassBuilder
{
    class Options
    {
        public bool Recursive { get; set; } = false;
        public List<string> Srcs { get; } = new List<string>();
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
                    options.Srcs.Add(str);
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
                    ProcessDirectory(src, options.Recursive);
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

        static void ProcessDirectory(string directory, bool recursive)
        {
            foreach (var file in Directory.GetFiles(directory, "*.scss", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                ProcessFile(file);
            }
        }
    }
}
