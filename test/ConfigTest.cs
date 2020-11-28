using Xunit;

namespace CssBuilder.Test
{
    public class ConfigTest : TestBase
    {
        [Fact]
        public void InvalidConfigFile()
        {
            var testFolder = PrepareTestFolder("ConfigTest_InvalidConfigFileTest");

            var exitCode = CssBuilder.Program.Main(new string[] { testFolder });
            Assert.Equal(1, exitCode);
        }

        [Fact]
        public void SingleInputNoOutputSetting()
        {
            var testFolder = PrepareTestFolder("ConfigTest_SingleInputNoOutputSetting");

            var exitCode = CssBuilder.Program.Main(new string[] { testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, "a.less", "a.css", "body {\n  color: red;\n}\n");
            VerifyFiles(testFolder, "b.less", "b.css", null);
            VerifyFiles(testFolder, "c.sass", "c.css", null);
        }

        [Fact]
        public void SingleInputOutputIsFolder()
        {
            var testFolder = PrepareTestFolder("ConfigTest_SingleInputOutputIsFolder");

            var exitCode = CssBuilder.Program.Main(new string[] { testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, "a.less", "a.css", null);
            VerifyFiles(testFolder, "a.less", "out/overall.css", "body {\n  color: red;\n}\n");
        }

        [Fact]
        public void GlobInputNoOutputSetting()
        {
            var testFolder = PrepareTestFolder("ConfigTest_GlobInputNoOutputSetting");

            var exitCode = CssBuilder.Program.Main(new string[] { testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, "a.less", "a.css", "body {\n  color: red;\n}\n");
            VerifyFiles(testFolder, "b/b.less", "b/b.css", "body {\n  color: blue;\n}\n");
            VerifyFiles(testFolder, "b/c/c.less", "b/c/c.css", "body {\n  color: yellow;\n}\n");
        }

        [Fact]
        public void GlobInputOutputIsFolder()
        {
            var testFolder = PrepareTestFolder("ConfigTest_GlobInputOutputIsFolder");

            var exitCode = CssBuilder.Program.Main(new string[] { testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, "a.less", "out/a.css", "body {\n  color: red;\n}\n");
            VerifyFiles(testFolder, "b/b.less", "out/b.css", "body {\n  color: blue;\n}\n");
            VerifyFiles(testFolder, "b/c/c.less", "out/c.css", "body {\n  color: yellow;\n}\n");
        }

        [Fact]
        public void MultipleConfig()
        {
            var testFolder = PrepareTestFolder("ConfigTest_MultipleConfig");

            var exitCode = CssBuilder.Program.Main(new string[] { testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, "a.less", "out/overall.css", "body {\n  color: red;\n}\n");
            VerifyFiles(testFolder, "b/b.less", "out/b.css", "body {\n  color: blue;\n}\n");
            VerifyFiles(testFolder, "b/c/c.less", "out/c.css", "body {\n  color: yellow;\n}\n");
        }

        [Fact]
        public void InputNotExist()
        {
            var testFolder = PrepareTestFolder("ConfigTest_InputNotExist");

            var exitCode = CssBuilder.Program.Main(new string[] { testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, "a.less", "a.css", null);
            VerifyFiles(testFolder, "b.less", "b.css", null);
            VerifyFiles(testFolder, "c.sass", "c.css", null);
        }
    }
}
