using LibSassHost;
using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace CssBuilder
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

    class CssBuilderConfigJson
    {
        /// <summary>
        /// Can be a single filename or a glob of pattern.
        /// If this option is not existed, all supported files will be processed.
        /// </summary>
        public object src { get; set; }

        /// <summary>
        /// Can be a single filename (if src is a single filename also).
        /// If this options is not existed, files will be output in the same folder.
        /// </summary>
        public string output { get; set; }

        public string _workdingDirectory { get; set; }

        public List<string> _srcs { get; set; }
    }

    public class Program
    {
        public static int Main(string[] args)
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
                Console.Error.WriteLine($"Error: {ex.Message}");
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
            var defaultConfig = new CssBuilderConfigJson();
            foreach (var src in options.Srcs)
            {
                if (File.Exists(src))
                {
                    ProcessFile(src, defaultConfig);
                }
                else if (Directory.Exists(src))
                {
                    defaultConfig._workdingDirectory = src;
                    ProcessDirectory(src, options.Recursive, options.Excludes == null ? GetExcludesFromGitIgnore(src) : options.Excludes, defaultConfig);
                }
                else
                {
                    throw new FileNotFoundException($"Can't find file ${src}");
                }
            }
        }

        static void ProcessFile(string file, CssBuilderConfigJson config)
        {
            string result = null;
            if (file.EndsWith(".less"))
            {
                result = CompileLessFile(file);
            }
            else if (file.EndsWith(".sass") || file.EndsWith(".scss"))
            {
                result = SassCompiler.CompileFile(file).CompiledContent;
            }

            if (result != null)
            {
                if (config.output == null)
                {
                    File.WriteAllText(Path.ChangeExtension(file, ".css"), result);
                }
                else if (config.output.EndsWith(".css"))
                {
                    File.AppendAllText(config.output, result);
                }
                else
                {
                    var fullName = Path.Combine(config._workdingDirectory, config.output, Path.GetFileNameWithoutExtension(file) + ".css");
                    Directory.CreateDirectory(Path.GetDirectoryName(fullName));
                    File.WriteAllText(fullName, result);
                }
            }
        }

        static void ProcessDirectory(string directory, bool recursive, IEnumerable<string> excludes, CssBuilderConfigJson config)
        {
            if (excludes.Contains(directory + Path.DirectorySeparatorChar))
            {
                return;
            }

            // If there is a "cssbuilder.config.json" file, use it to override the default one.
            List<CssBuilderConfigJson> configs = null;
            var cssBuilderConfigFile = Path.Combine(directory, "cssbuilder.config.json");
            if (File.Exists(cssBuilderConfigFile))
            {
                try
                {
                    var workingDirectory = Path.GetDirectoryName(cssBuilderConfigFile);
                    configs = JsonSerializer.Deserialize<List<CssBuilderConfigJson>>(File.ReadAllText(cssBuilderConfigFile));
                    foreach (var c in configs)
                    {
                        NormalizeConfig(c, workingDirectory);
                    }
                }
                catch(JsonException)
                {
                    Console.Error.WriteLine($"Parse configure file: {cssBuilderConfigFile} failed");
                    throw;
                }
            }

            if (configs == null)
            {
                configs = new List<CssBuilderConfigJson>() { config };
            }

            foreach (var c in configs)
            {
                if (c._srcs == null)
                {
                    foreach (var ext in new[] { "*.less", "*.sass", "*.scss", })
                    {
                        foreach (var file in Directory.GetFiles(directory, ext, SearchOption.TopDirectoryOnly))
                        {
                            if (!excludes.Contains(file))
                            {
                                ProcessFile(file, c);
                            }
                        }
                    }

                    if (recursive)
                    {
                        foreach (var dir in Directory.GetDirectories(directory))
                        {
                            ProcessDirectory(dir, recursive, excludes, c);
                        }
                    }
                }
                else
                {
                    var matcher = new Matcher();
                    matcher.AddIncludePatterns(c._srcs);
                    foreach (var file in matcher.GetResultsInFullPath(directory).OrderBy(f => f))
                    {
                        if (!excludes.Contains(file))
                        {
                            ProcessFile(file, c);
                        }
                    }
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
                if (Path.DirectorySeparatorChar != '/')
                {
                    output = output.Replace('/', Path.DirectorySeparatorChar);
                }
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

        static void NormalizeConfig(CssBuilderConfigJson config, string workingDirectory)
        {
            config._workdingDirectory = workingDirectory;

            if (config.src != null)
            {
                config._srcs = new List<string>();

                var element = (JsonElement)config.src;
                if (element.ValueKind == JsonValueKind.String)
                {
                    config._srcs.Add(element.GetString());
                }
                else if (element.ValueKind == JsonValueKind.Array)
                {
                    for (var i = 0; i < element.GetArrayLength(); ++i)
                    {
                        var e = element[i];
                        if (e.ValueKind != JsonValueKind.String)
                        {
                            throw new JsonException("`src` must be string or array of strings.");
                        }
                        config._srcs.Add(e.GetString());
                    }
                }
                else
                {
                    throw new JsonException("`src` must be string or array of strings.");
                }
            }

            if (config.output != null)
            {
                if (config.output.EndsWith(".css"))
                {
                    var fullName = Path.Combine(config._workdingDirectory, config.output);
                    Directory.CreateDirectory(Path.GetDirectoryName(fullName));
                    File.Delete(fullName);
                    config.output = fullName;
                }
            }
        }

        static string CompileLessFile(string file)
        {
            LessEngine.CurrentDirectory = Path.GetDirectoryName(file);
            return LessEngine.TransformToCss(File.ReadAllText(file), file);
        }

        static void LogDebug(string str)
        {
#if DEBUG
            Console.WriteLine("[CssBuilder]: " + str);
#endif
        }

        static dotless.Core.ILessEngine _lessEngine;
        static dotless.Core.ILessEngine LessEngine
        {
            get
            {
                if (_lessEngine == null)
                {
                    _lessEngine = new dotless.Core.EngineFactory().GetEngine();
                }
                return _lessEngine;
            }
        }
    }
}
