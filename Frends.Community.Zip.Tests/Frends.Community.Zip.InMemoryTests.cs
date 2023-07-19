using NUnit.Framework;
using System.IO;
using System.Threading;
using System.Text;

namespace Frends.Community.Zip.InMemoryTests
{
    [TestFixture]
    public class InMemoryTests
    {
        private static readonly string _basePath = Path.Combine(Path.GetTempPath(), "frends.community.zip.inmemorytests");

        private static readonly string _outPath = Path.Combine(_basePath, $"TestOut{Path.DirectorySeparatorChar}");

        private static readonly string _zipFilePath = Path.Combine(_basePath, "zip_test.zip");

        private static readonly string[] mockFiles =
        { 
            @"This is supposed to represent a text file.",
            @"The quick brown fox jumps over the lazy dog. Yet another text file.",
            @"This text file contains some scandinavian letters: äöå ÄÖÅ. There should have been some legible letters before this."
        };

        MemorySource testSource;

        MemoryOptions testOptions;
        MemoryOptions testEncodingOptions;

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
                            FileName = $"folder{Path.DirectorySeparatorChar}test2.txt",
                            FileContent = Encoding.UTF8.GetBytes(mockFiles[1])
                        },
                        new MemoryFiles
                        {
                            FileName = "test3_äöå.txt",
                            FileContent = Encoding.UTF8.GetBytes(mockFiles[2])
                        }
                    }
            };

            testOptions = new MemoryOptions();

            testEncodingOptions = new MemoryOptions()
            {
                FileEncoding = FileEncoding.UTF8
            };

            Directory.CreateDirectory(_basePath);
        }

        [Test]
        public void ZipInMemory()
        {
            var result = ZipTask.CreateArchiveInMemory(testSource, testOptions, CancellationToken.None);

            Assert.IsNotNull(result.ResultBytes);

            File.WriteAllBytes(_zipFilePath, result.ResultBytes);

            var unzipInput = new UnzipInputProperties
            {
                DestinationDirectory = _outPath,
                SourceFile = _zipFilePath
            };
            var unzipOptions = new UnzipOptions
            {
                CreateDestinationDirectory = true
            };

            ZipTask.ExtractArchive(unzipInput, unzipOptions, CancellationToken.None);

            Assert.True(File.Exists(Path.Combine(_outPath, "test1.txt")));
            Assert.True(File.Exists(Path.Combine(_outPath, $"folder{Path.DirectorySeparatorChar}test2.txt")));
            Assert.True(File.Exists(Path.Combine(_outPath, "test3_äöå.txt")));
        }

        [Test]
        public void ZipInMemory_Encoding()
        {
            var result = ZipTask.CreateArchiveInMemory(testSource, testEncodingOptions, CancellationToken.None);

            Assert.IsNotNull(result.ResultBytes);

            File.WriteAllBytes(_zipFilePath, result.ResultBytes);

            var unzipInput = new UnzipInputProperties
            {
                DestinationDirectory = _outPath,
                SourceFile = _zipFilePath
            };
            var unzipOptions = new UnzipOptions
            {
                CreateDestinationDirectory = true
            };

            ZipTask.ExtractArchive(unzipInput, unzipOptions, CancellationToken.None);

            Assert.True(File.Exists(Path.Combine(_outPath, "test1.txt")));
            Assert.True(File.ReadAllText(Path.Combine(_outPath, "test1.txt"), Encoding.UTF8) == mockFiles[0]);

            Assert.True(File.Exists(Path.Combine(_outPath, $"folder{Path.DirectorySeparatorChar}test2.txt")));
            Assert.True(File.ReadAllText(Path.Combine(_outPath, $"folder{Path.DirectorySeparatorChar}test2.txt"), Encoding.UTF8) == mockFiles[1]);

            Assert.True(File.Exists(Path.Combine(_outPath, "test3_äöå.txt")));
            Assert.True(File.ReadAllText(Path.Combine(_outPath, "test3_äöå.txt"), Encoding.UTF8) == mockFiles[2]);
        }
    }
}
