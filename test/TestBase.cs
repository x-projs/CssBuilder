using System;
using System.IO;
using Xunit;

namespace CssBuilder.Test
{
    [Collection("Sequential")]
    public class TestBase
    {
        protected string PrepareTestFolder(string testName)
        {
            string testFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestTemp", testName);
            if (Directory.Exists(testFolder))
            {
                foreach (var info in new DirectoryInfo(testFolder).GetFileSystemInfos("*", SearchOption.AllDirectories))
                {
                    info.Attributes = FileAttributes.Normal;
                }
                Directory.Delete(testFolder, recursive: true);
            }

            string testDataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", testName);
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestTemp", testName));
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

        protected void VerifyFiles(string testFolder, string originalFile, string resultFile, string expectedContent)
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
    }
}
