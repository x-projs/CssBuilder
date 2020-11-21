using System;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace SassBuilder.Test
{
    public class BasicTest
    {
        [Fact]
        public void RecursiveTest()
        {
            var testFolder = PrepareTestFolder("RecursiveTest");
            var exitCode = SassBuilder.Program.Main(new string[] { "-r", testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, new (string, string, string)[]
            {
                ("test.scss", "test.css", "body {\n  color: red; }\n"),
                ("sub-folder/test.scss", "sub-folder/test.css", "body {\n  color: blue; }\n"),
                ("sub-folder/sub-folder/test.scss", "sub-folder/sub-folder/test.css", "body {\n  color: yellow; }\n")
            });
        }

        [Fact]
        public void NonRecursiveTest()
        {
            var testFolder = PrepareTestFolder("RecursiveTest");
            var exitCode = SassBuilder.Program.Main(new string[] { testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, new (string, string, string)[]
            {
                ("test.scss", "test.css", "body {\n  color: red; }\n"),
                ("sub-folder/test.scss", "sub-folder/test.css", null),
                ("sub-folder/sub-folder/test.scss", "sub-folder/sub-folder/test.css", null)
            });
        }

        [Fact]
        public void GitIgnoreTest()
        {
            var testFolder = PrepareTestFolder("GitIgnoreTest");
            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = "git",
                Arguments = "init",
                WorkingDirectory = testFolder
            });
            process.WaitForExit();
            var exitCode = process.ExitCode;
            Assert.Equal(0, exitCode);

            process = Process.Start(new ProcessStartInfo()
            {
                FileName = "git",
                Arguments = "add .",
                WorkingDirectory = testFolder
            });
            process.WaitForExit();
            exitCode = process.ExitCode;
            Assert.Equal(0, exitCode);

            exitCode = SassBuilder.Program.Main(new string[] { "-r", testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, new (string, string, string)[]
            {
                ("a.scss", "a.css", "body {\n  color: red; }\n"),
                ("b.scss", "b.css", null),
                ("sub-folder/test.scss", "sub-folder/test.css", "body {\n  color: blue; }\n"),
                ("sub-folder/ignored-folder/test.scss", "sub-folder/ignored-folder/test.css", null)
            });
        }

        private void VerifyFiles(string testFolder, (string originalFile, string resultFile, string expectedContent)[] verifyItems)
        {
            foreach (var item in verifyItems)
            {
                Assert.True(File.Exists(Path.Combine(testFolder, item.originalFile)));

                var resultFile = Path.Combine(testFolder, item.resultFile);
                if (item.expectedContent != null)
                {
                    Assert.True(File.Exists(resultFile));
                    Assert.Equal(item.expectedContent, File.ReadAllText(resultFile));
                }
                else
                {
                    Assert.False(File.Exists(resultFile));
                }
            }
        }

        private string PrepareTestFolder(string testName)
        {
            string testFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestTmp", testName);
            if (Directory.Exists(testFolder))
            {
                foreach (var info in new DirectoryInfo(testFolder).GetFileSystemInfos("*", SearchOption.AllDirectories))
                {
                    info.Attributes = FileAttributes.Normal;
                }
                Directory.Delete(testFolder, recursive: true);
            }

            string testDataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", testName);
            foreach (string dirPath in Directory.GetDirectories(testDataFolder, "*", SearchOption.AllDirectories)) {
                Directory.CreateDirectory(dirPath.Replace(testDataFolder, testFolder));
            }
            foreach (string newPath in Directory.GetFiles(testDataFolder, "*.*", SearchOption.AllDirectories)) {
                File.Copy(newPath, newPath.Replace(testDataFolder, testFolder));
            }

            return testFolder;
        }
    }
}
