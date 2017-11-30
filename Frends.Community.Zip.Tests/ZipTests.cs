using System;
using System.IO;
using NUnit.Framework;
using Frends.Community.Zip;
using System.Threading;
using System.Linq;

namespace FRENDS.Community.Zip.Tests
{
    [TestFixture]
    public class ZipTests
    {
        private static readonly string _basePath = Path.Combine(Path.GetTempPath(), "frends.community.zip.tests");

        private readonly string _dirIn = Path.Combine(_basePath, @"In\");
        private readonly string _subDir = "Subdir";
        private string _subDirIn;
        private readonly string _dirOut = Path.Combine(_basePath, @"Out\");
        private readonly string _zipFileName = "zip_test.zip";

        private SourceProperties _source;
        private DestinationProperties _destination;
        private Options _options;
        
        [TearDown]
        public void TearDown()
        {
            // remove test directories and files
            Directory.Delete(_basePath, true);
        }

        [SetUp]
        public void SetupTests()
        {
            _subDirIn = Path.Combine(_dirIn, _subDir);
            _source = new SourceProperties { Path = _dirIn, FileMask = "*.txt", IncludeSubFolders = false };
            _destination = new DestinationProperties
            {
                Path = _dirOut,
                FileName = _zipFileName,
                CreateDestinationFolder = false,
                FlattenFolders = false,
                Password = "",
                RenameDublicateFiles = false
            };
            _options = new Options { ThrowErrorIfNoFilesFound = true };

            // create source directoty and files
            Directory.CreateDirectory(_dirIn);
            File.WriteAllText(Path.Combine(_dirIn, "test_1_file.txt"), "foobaar foobar");
            File.WriteAllText(Path.Combine(_dirIn, "test_2_file.txt"), "foobaar foobar");
            // create sub dir for recursive test
            Directory.CreateDirectory(_subDirIn);
            File.WriteAllText(Path.Combine(_subDirIn, "sub_test_1_file.txt"), "foobaar foobar");
            File.WriteAllText(Path.Combine(_subDirIn, "sub_test_2_file.txt"), "foobaar foobar");

            Directory.CreateDirectory(_dirOut);
        }

        private Output ExecuteCreateArchive()
        {
            return Frends.Community.Zip.Zip.CreateArchive(_source, _destination, _options, CancellationToken.None);
        }

        [Test]
        public void ZipFiles_NonRecursive()
        {
            var result = ExecuteCreateArchive();

            Assert.AreEqual(_zipFileName, result.FileName);
            Assert.AreEqual(2, result.FileCount);
            Assert.That(File.Exists(Path.Combine(_destination.Path, _zipFileName)));
        }

        [Test]
        public void ZipFiles_Fails_If_SourcePathDoesNotExist()
        {
            _source.Path = Path.Combine(_dirIn, "foobar");

            var result = Assert.Throws<DirectoryNotFoundException>(() => ExecuteCreateArchive());

            Assert.IsTrue(result.Message.Contains("Source directory"));
            Assert.IsTrue(result.Message.Contains("does not exist."));
        }

        [Test]
        public void Zipfiles_DoesNotFail_IfSourceFilesAreNotFound()
        {
            _source.FileMask = "foobar.txt";
            _options.ThrowErrorIfNoFilesFound = false;

            var result = ExecuteCreateArchive();

            Assert.AreEqual(0, result.FileCount);
            Assert.That(!File.Exists(Path.Combine(_destination.Path, _zipFileName)));
        }

        [Test]
        public void ZipFiles_Fails_IfNoSourceFilesFound()
        {
            _source.FileMask = "foobar.txt";
            var result = Assert.Throws<FileNotFoundException>(() => ExecuteCreateArchive());
        }

        [Test]
        public void ZipFiles_Fails_If_DestinationPathDoesNotExist()
        {
            _destination.Path = Path.Combine(_destination.Path, "foobar");

            var result = Assert.Throws<DirectoryNotFoundException>(() => ExecuteCreateArchive());

            Assert.IsTrue(result.Message.Contains("Destination directory"));
            Assert.IsTrue(result.Message.Contains("does not exist."));
        }

        [Test]
        public void ZipFiles_NonRecursive_And_CreateDestinationDirectory()
        {
            _destination.CreateDestinationFolder = true;
            _destination.Path = Path.Combine(_dirOut, "newDir");

            var result = ExecuteCreateArchive();

            Assert.AreEqual(_zipFileName, result.FileName);
            Assert.AreEqual(2, result.FileCount);
            Assert.That(File.Exists(Path.Combine(_destination.Path, _zipFileName)));
        }
        

        [Test]
        public void ZipFiles_Recursive()
        {
            _source.IncludeSubFolders = true;

            var result = ExecuteCreateArchive();

            Assert.AreEqual(_zipFileName, result.FileName);
            Assert.AreEqual(4, result.FileCount);
            Assert.That(File.Exists(Path.Combine(_destination.Path, _zipFileName)));
            var fileNamesWithSubDir = result.ArchivedFiles.Where(s => s.Contains(_subDir)).Count();
            Assert.AreEqual(2, fileNamesWithSubDir);
        }

        [Test]
        public void ZipFiles_FlattenFolders()
        {
            _source.IncludeSubFolders = true;
            _destination.FlattenFolders = true;

            var result = ExecuteCreateArchive();

            Assert.AreEqual(_zipFileName, result.FileName);
            Assert.AreEqual(4, result.FileCount);
            var subDirIsPresent = result.ArchivedFiles.Where(s => s.Contains(_subDir)).Count() > 0;
            Assert.IsFalse(subDirIsPresent);
        }

        [Test]
        public void ZipFiles_FlattenFolders_Fails_IfDublicateFileNames_And_RenameFalse()
        {
            var dublicateFileName = "dublicate_file.txt";
            // create files with dublicate names in separate folders
            File.WriteAllText(Path.Combine(_dirIn, dublicateFileName), "Seaman: Swallow, come!");
            File.WriteAllText(Path.Combine(_subDirIn, dublicateFileName), "Seaman: Swallow, come!");

            _source.IncludeSubFolders = true;
            _destination.FlattenFolders = true;

            var result = Assert.Throws<Exception>(() => ExecuteCreateArchive());
            Assert.IsTrue(result.Message.Contains("already exists in zip!"));
        }

        [Test]
        public void ZipFiles_Flattenfolders_RenamesDublicateFiles()
        {
            var dublicateFileName = "dublicate_file.txt";
            // create files with dublicate names in separate folders
            File.WriteAllText(Path.Combine(_dirIn, dublicateFileName), "Seaman: Swallow, come!");
            File.WriteAllText(Path.Combine(_subDirIn, dublicateFileName), "Seaman: Swallow, come!");
            var subDir2 = Path.Combine(_subDirIn, "subdir2");
            Directory.CreateDirectory(subDir2);
            File.WriteAllText(Path.Combine(subDir2, dublicateFileName), "Seaman: Swallow, come!");

            _source.IncludeSubFolders = true;
            _destination.FlattenFolders = true;
            _destination.RenameDublicateFiles = true;

            var result = ExecuteCreateArchive();

            Assert.AreEqual(_zipFileName, result.FileName);
            Assert.AreEqual(7, result.FileCount);
            var subDirCount = result.ArchivedFiles.Where(s => s.Contains(_subDir)).Count();
            Assert.AreEqual(0, subDirCount);
            Assert.Contains("dublicate_file.txt", result.ArchivedFiles);
            Assert.Contains("dublicate_file_(1).txt", result.ArchivedFiles);
            Assert.Contains("dublicate_file_(2).txt", result.ArchivedFiles);
        }

        [Test]
        public void ZipFile_Exists_ThrowsError()
        {
            _options.DestinationFileExistsAction = FileExistAction.Error;

            ExecuteCreateArchive();
            var result = Assert.Throws<Exception>(() => ExecuteCreateArchive());

            Assert.IsTrue(result.Message.Equals($"Destination file {Path.Combine(_destination.Path, _destination.FileName)} already exists."));
        }

        [Test]
        public void ZipFile_Exists_OverwriteFile()
        {
            _options.DestinationFileExistsAction = FileExistAction.Overwrite;

            ExecuteCreateArchive();
            ExecuteCreateArchive();

            Assert.AreEqual(1, Directory.GetFiles(_destination.Path, "*.zip").Count());
        }

        [Test]
        public void ZipFile_Exists_Rename()
        {
            _options.DestinationFileExistsAction = FileExistAction.Rename;

            var result1 = ExecuteCreateArchive();
            var result2 = ExecuteCreateArchive();
            var result3 = ExecuteCreateArchive();

            var zipFiles = Directory.GetFiles(_destination.Path, "*.zip");

            Assert.AreEqual(3, zipFiles.Count());
            Assert.AreEqual("zip_test.zip", result1.FileName);
            Assert.AreEqual("zip_test_(1).zip", result2.FileName);
            Assert.AreEqual("zip_test_(2).zip", result3.FileName);
        }

        //TODO: Open zip protected with password
        [Test]
        [Ignore("This does not actully test the password, yet!")]
        public void ZipFiles_WithPassword_NeedsPasword_For_Extraction()
        {
            _destination.Password = "password";
            var result = ExecuteCreateArchive();

            Assert.AreEqual(_zipFileName, result.FileName);
            Assert.AreEqual(2, result.FileCount);
            Assert.That(File.Exists(Path.Combine(_destination.Path, _zipFileName)));
        }
    }
}

