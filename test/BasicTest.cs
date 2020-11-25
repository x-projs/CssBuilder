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

            // Verify *.less
            VerifyFiles(testFolder, "less-test.less", "less-test.css", "body {\n  color: red;\n}\n");
            VerifyFiles(testFolder, "sub-folder/less-test.less", "sub-folder/less-test.css", "body {\n  color: blue;\n}\n");
            VerifyFiles(testFolder, "sub-folder/sub-folder/less-test.less", "sub-folder/sub-folder/less-test.css", "body {\n  color: yellow;\n}\n");

            // Verify *.sass
            VerifyFiles(testFolder, "sass-test.sass", "sass-test.css", "body {\n  color: red; }\n");
            VerifyFiles(testFolder, "sub-folder/sass-test.sass", "sub-folder/sass-test.css", "body {\n  color: blue; }\n");
            VerifyFiles(testFolder, "sub-folder/sub-folder/sass-test.sass", "sub-folder/sub-folder/sass-test.css", "body {\n  color: yellow; }\n");

            // Verify *.scss
            VerifyFiles(testFolder, "test.scss", "test.css", "body {\n  color: red; }\n");
            VerifyFiles(testFolder, "sub-folder/test.scss", "sub-folder/test.css", "body {\n  color: blue; }\n");
            VerifyFiles(testFolder, "sub-folder/sub-folder/test.scss", "sub-folder/sub-folder/test.css", "body {\n  color: yellow; }\n");
        }

        [Fact]
        public void NonRecursiveTest()
        {
            var testFolder = PrepareTestFolder("RecursiveTest");
            var exitCode = SassBuilder.Program.Main(new string[] { testFolder });
            Assert.Equal(0, exitCode);

            // Verify compile *.less
            VerifyFiles(testFolder, "less-test.less", "less-test.css", "body {\n  color: red;\n}\n");
            VerifyFiles(testFolder, "sub-folder/less-test.less", "sub-folder/less-test.css", null);
            VerifyFiles(testFolder, "sub-folder/sub-folder/less-test.less", "sub-folder/sub-folder/less-test.css", null);

            // Verify compile *.sass
            VerifyFiles(testFolder, "sass-test.sass", "sass-test.css", "body {\n  color: red; }\n");
            VerifyFiles(testFolder, "sub-folder/sass-test.sass", "sub-folder/sass-test.css", null);
            VerifyFiles(testFolder, "sub-folder/sub-folder/sass-test.sass", "sub-folder/sub-folder/sass-test.css", null);

            // Verify compile *.scss
            VerifyFiles(testFolder, "test.scss", "test.css", "body {\n  color: red; }\n");
            VerifyFiles(testFolder, "sub-folder/test.scss", "sub-folder/test.css", null);
            VerifyFiles(testFolder, "sub-folder/sub-folder/test.scss", "sub-folder/sub-folder/test.css", null);
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

            VerifyFiles(testFolder, "a.scss", "a.css", "body {\n  color: red; }\n");
            VerifyFiles(testFolder, "sass-a.sass", "sass-a.css", "body {\n  color: red; }\n");
            VerifyFiles(testFolder, "b.scss", "b.css", null);
            VerifyFiles(testFolder, "sass-b.sass", "sass-b.css", null);
            VerifyFiles(testFolder, "sub-folder/test.scss", "sub-folder/test.css", "body {\n  color: blue; }\n");
            VerifyFiles(testFolder, "sub-folder/sass-test.sass", "sub-folder/sass-test.css", "body {\n  color: blue; }\n");
            VerifyFiles(testFolder, "sub-folder/ignored-folder/test.scss", "sub-folder/ignored-folder/test.css", null);
            VerifyFiles(testFolder, "sub-folder/ignored-folder/sass-test.sass", "sub-folder/ignored-folder/sass-test.css", null);
            VerifyFiles(testFolder, "sub-folder/ignore-scss/test.scss", "sub-folder/ignore-scss/test.css", null);
            VerifyFiles(testFolder, "sub-folder/ignore-scss/sass-test.sass", "sub-folder/ignore-scss/sass-test.css", "body {\n  color: red; }\n");
            VerifyFiles(testFolder, "sub-folder/ignore-sass/test.scss", "sub-folder/ignore-sass/test.css", "body {\n  color: red; }\n");
            VerifyFiles(testFolder, "sub-folder/ignore-sass/sass-test.sass", "sub-folder/ignore-sass/sass-test.css", null);
        }

        private void VerifyFiles(string testFolder, string originalFile, string resultFile, string expectedContent)
        {
            Assert.True(File.Exists(Path.Combine(testFolder, originalFile)));

            resultFile = Path.Combine(testFolder, resultFile);
            if (expectedContent != null)
            {
                Assert.True(File.Exists(resultFile));
                Assert.Equal(expectedContent, File.ReadAllText(resultFile));
            }
            else
            {
                Assert.False(File.Exists(resultFile));
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
            foreach (string dirPath in Directory.GetDirectories(testDataFolder, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(testDataFolder, testFolder));
            }
            foreach (string newPath in Directory.GetFiles(testDataFolder, "*.*", SearchOption.AllDirectories))
            {
                if (newPath.EndsWith(".gitignore.template"))
                {
                    File.Copy(newPath, newPath.Replace(".gitignore.template", ".gitignore").Replace(testDataFolder, testFolder));
                }
                else
                {
                    File.Copy(newPath, newPath.Replace(testDataFolder, testFolder));
                }
            }

            return testFolder;
        }
    }
}
