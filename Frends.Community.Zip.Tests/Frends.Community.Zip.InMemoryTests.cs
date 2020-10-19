using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Ionic.Zip;
using System.Text;

namespace Frends.Community.Zip.InMemoryTests
{
    [TestFixture]
    public class InMemoryTests
    {
        private static readonly string _basePath = Path.Combine(Path.GetTempPath(), "frends.community.zip.inmemorytests");

        private static readonly string _outPath = Path.Combine(_basePath, @"TestOut\");

        private static readonly string _zipFilePath = Path.Combine(_basePath, "zip_test.zip");

        private static readonly string[] mockFiles = { 
            @"This is supposed to represent a text file.",
            @"The quick brown fox jumps over the lazy dog. Yet another text file."
        };

        MemorySource testSource;

        MemoryOptions testOptions;

        [TearDown]
        public void TearDown()
        {
            // Remove any test directories and files
            Directory.Delete(_basePath, true);
        }

        [SetUp]
        public void SetupTests()
        {
            testSource = new MemorySource
            {
                SourceFiles = new MemoryFiles[]
                    {
                        new MemoryFiles
                        {
                            FileName = "test1.txt",
                            FileContent = Encoding.UTF8.GetBytes(mockFiles[0])
                        },
                        new MemoryFiles
                        {
                            FileName = "folder/test2.txt",
                            FileContent = Encoding.UTF8.GetBytes(mockFiles[1])
                        }
                    }
            };

            testOptions = new MemoryOptions();

            Directory.CreateDirectory(_basePath);
        }

        [Test]
        public void ZipInMemory()
        {
            var result = ZipTask.CreateArchiveInMemory(testSource, testOptions, CancellationToken.None);

            Assert.IsNotNull(result.ResultBytes);

            File.WriteAllBytes(_zipFilePath, result.ResultBytes);

            var unzipInput = new UnzipInputProperties {
                DestinationDirectory = _outPath,
                SourceFile = _zipFilePath
            };
            var unzipOptions = new UnzipOptions {
                CreateDestinationDirectory = true
            };

            ZipTask.ExtractArchive(unzipInput, unzipOptions, CancellationToken.None);

            Assert.True(File.Exists(Path.Combine(_outPath, "test1.txt")));
            Assert.True(File.Exists(Path.Combine(_outPath, "folder/test2.txt")));
        }
    }
}
