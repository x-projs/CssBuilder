using System.Diagnostics;
using System.IO;
using Xunit;

namespace CssBuilder.Test
{
    public class BasicTest : TestBase
    {
        [Fact]
        public void RecursiveTest()
        {
            var testFolder = PrepareTestFolder("RecursiveTest");
            var exitCode = Program.Main(new string[] { "-r", testFolder });
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
            var exitCode = Program.Main(new string[] { testFolder });
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

            exitCode = Program.Main(new string[] { "-r", testFolder });
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
    }
}
