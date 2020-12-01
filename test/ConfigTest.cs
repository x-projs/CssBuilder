using Xunit;

namespace CssBuilder.Test
{
    public class ConfigTest : TestBase
    {
        [Fact]
        public void InvalidConfigFile()
        {
            var testFolder = PrepareTestFolder("ConfigTest_InvalidConfigFileTest");

            var exitCode = Program.Main(new string[] { testFolder });
            Assert.Equal(1, exitCode);
        }

        [Fact]
        public void SingleInputNoOutputSetting()
        {
            var testFolder = PrepareTestFolder("ConfigTest_SingleInputNoOutputSetting");

            var exitCode = Program.Main(new string[] { testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, "a.less", "a.css", "body {\n  color: red;\n}\n");
            VerifyFiles(testFolder, "b.less", "b.css", null);
            VerifyFiles(testFolder, "c.sass", "c.css", null);
        }

        [Fact]
        public void SingleInputOutputIsFile()
        {
            var testFolder = PrepareTestFolder("ConfigTest_SingleInputOutputIsFile");

            var exitCode = Program.Main(new string[] { testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, "a.less", "a.css", null);
            VerifyFiles(testFolder, "a.less", "out/overall.css", "body {\n  color: red;\n}\n");
        }

        [Fact]
        public void SingleInputOutputIsFolder()
        {
            var testFolder = PrepareTestFolder("ConfigTest_SingleInputOutputIsFolder");

            var exitCode = Program.Main(new string[] { testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, "a.less", "a.css", null);
            VerifyFiles(testFolder, "a.less", "out/a.css", "body {\n  color: red;\n}\n");
        }

        [Fact]
        public void GlobInputNoOutputSetting()
        {
            var testFolder = PrepareTestFolder("ConfigTest_GlobInputNoOutputSetting");

            var exitCode = Program.Main(new string[] { testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, "a.less", "a.css", "body {\n  color: red;\n}\n");
            VerifyFiles(testFolder, "b/b.less", "b/b.css", "body {\n  color: blue;\n}\n");
            VerifyFiles(testFolder, "b/c/c.less", "b/c/c.css", "body {\n  color: yellow;\n}\n");
        }

        [Fact]
        public void GlobInputOutputIsFile()
        {
            var testFolder = PrepareTestFolder("ConfigTest_GlobInputOutputIsFile");

            var exitCode = Program.Main(new string[] { testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, "a.less", "out/a.css", null);
            VerifyFiles(testFolder, "b/b.less", "out/b.css", null);
            VerifyFiles(testFolder, "b/c/c.less", "out/c.css", null);
            VerifyFiles(testFolder, null, "out/overall.css", "body {\n  color: red;\n}\nbody {\n  color: blue;\n}\nbody {\n  color: yellow;\n}\n");
        }

        [Fact]
        public void GlobInputOutputIsFolder()
        {
            var testFolder = PrepareTestFolder("ConfigTest_GlobInputOutputIsFolder");

            var exitCode = Program.Main(new string[] { testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, "a.less", "out/a.css", "body {\n  color: red;\n}\n");
            VerifyFiles(testFolder, "b/b.less", "out/b.css", "body {\n  color: blue;\n}\n");
            VerifyFiles(testFolder, "b/c/c.less", "out/c.css", "body {\n  color: yellow;\n}\n");
        }

        [Fact]
        public void MultipleConfig()
        {
            var testFolder = PrepareTestFolder("ConfigTest_MultipleConfig");

            var exitCode = Program.Main(new string[] { testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, "a.less", "out/overall.css", "body {\n  color: red;\n}\n");
            VerifyFiles(testFolder, "b/b.less", "out/b.css", "body {\n  color: blue;\n}\n");
            VerifyFiles(testFolder, "b/c/c.less", "out/c.css", "body {\n  color: yellow;\n}\n");
        }

        [Fact]
        public void MultipleInputNoOutputSetting()
        {
            var testFolder = PrepareTestFolder("ConfigTest_MultipleInputNoOutputSetting");

            var exitCode = Program.Main(new string[] { testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, "a.less", "a.css", "body {\n  color: red;\n}\n");
            VerifyFiles(testFolder, "b.less", "b.css", "body {\n  color: blue;\n}\n");
            VerifyFiles(testFolder, "c.sass", "c.css", null);
        }

        [Fact]
        public void MultipleInputOutputIsFile()
        {
            var testFolder = PrepareTestFolder("ConfigTest_MultipleInputOutputIsFile");

            var exitCode = Program.Main(new string[] { testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, "a.less", "a.css", null);
            VerifyFiles(testFolder, "b.less", "b.css", null);
            VerifyFiles(testFolder, null, "out/overall.css", "body {\n  color: red;\n}\nbody {\n  color: blue;\n}\n");
        }


        [Fact]
        public void MultipleInputOutputIsFolder()
        {
            var testFolder = PrepareTestFolder("ConfigTest_MultipleInputOutputIsFolder");

            var exitCode = Program.Main(new string[] { testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, "a.less", "out/a.css", "body {\n  color: red;\n}\n");
            VerifyFiles(testFolder, "b/b.less", "out/b.css", null);
            VerifyFiles(testFolder, "b/c/c.less", "out/c.css", "body {\n  color: yellow;\n}\n");
        }

        [Fact]
        public void SingleAndGlobInputNoOutputSetting()
        {
            var testFolder = PrepareTestFolder("ConfigTest_SingleAndGlobInputNoOutputSetting");

            var exitCode = Program.Main(new string[] { testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, "a.less", "a.css", "body {\n  color: red;\n}\n");
            VerifyFiles(testFolder, "b/b.less", "b/b.css", "body {\n  color: blue;\n}\n");
            VerifyFiles(testFolder, "b/c/c.less", "b/c/c.css", "body {\n  color: yellow;\n}\n");
        }

        [Fact]
        public void SingleAndGlobInputOutputIsFile()
        {
            var testFolder = PrepareTestFolder("ConfigTest_SingleAndGlobInputOutputIsFile");

            var exitCode = Program.Main(new string[] { testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, "a.less", "a.css", null);
            VerifyFiles(testFolder, "b/b.less", "b/b.css", null);
            VerifyFiles(testFolder, "b/c/c.less", "b/c/c.css", null);
            VerifyFiles(testFolder, null, "overall.css", "body {\n  color: red;\n}\nbody {\n  color: blue;\n}\nbody {\n  color: yellow;\n}\n");
        }

        [Fact]
        public void SingleAndGlobInputOutputIsFolder()
        {
            var testFolder = PrepareTestFolder("ConfigTest_SingleAndGlobInputOutputIsFolder");

            var exitCode = Program.Main(new string[] { testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, "a.less", "out/a.css", "body {\n  color: red;\n}\n");
            VerifyFiles(testFolder, "b/b.less", "out/b.css", "body {\n  color: blue;\n}\n");
            VerifyFiles(testFolder, "b/c/c.less", "out/c.css", "body {\n  color: yellow;\n}\n");
        }

        [Fact]
        public void MinifyTest()
        {
            var testFolder = PrepareTestFolder("ConfigTest_MinifyTest");

            var exitCode = Program.Main(new string[] { testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, "a.less", "a.css", "body{color:red}");
            VerifyFiles(testFolder, "b.scss", "b.css", "body{color:blue}\n");
            VerifyFiles(testFolder, "c.sass", "c.css", "body {\n  color: yellow; }\n");
            VerifyFiles(testFolder, "d.styl", "d.css", "body{color:#f00}");
        }

        [Fact]
        public void InputNotExist()
        {
            var testFolder = PrepareTestFolder("ConfigTest_InputNotExist");

            var exitCode = Program.Main(new string[] { testFolder });
            Assert.Equal(0, exitCode);

            VerifyFiles(testFolder, "a.less", "a.css", null);
            VerifyFiles(testFolder, "b.less", "b.css", null);
            VerifyFiles(testFolder, "c.sass", "c.css", null);
        }
    }
}
