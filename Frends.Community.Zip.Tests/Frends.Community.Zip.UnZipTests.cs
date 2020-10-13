using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Ionic.Zip;

namespace Frends.Community.Zip.UnzipTests
{
    [TestFixture]
    public class UnZipTests
    {
        string[] fileNames = {@"logo1.png", @"logo2.png", @"folder\logo1.png", @"folder\logo2.png", @"folder\folder\folder\logo1.png",
                                @"folder\folder\folder\logo2.png", @"folder\folder\folder\folder\logo1.png" };
        //used for testing the rename-option
        string[] renamedFilenames = {@"logo1(0).png", @"logo2(0).png", @"folder\logo1(0).png", @"folder\logo2(0).png", @"folder\folder\folder\logo1(0).png",
                                @"folder\folder\folder\logo2(0).png", @"folder\folder\folder\folder\logo1(0).png" };

        //paths to TestIn and TestOut
        string inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\TestData\TestIn\");
        string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\TestData\TestOut\");
        List<string> outputFiles;
        UnzipInputProperties sp;
        UnzipOptions opt;

        [SetUp]
        public void Setup()
        {
            sp = new UnzipInputProperties();
            opt = new UnzipOptions();
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\TestData\TestOut\"));
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\TestData\TestOut"), true);
        }


        [Test]
        public void SourceFileDoesNotExist()
        {
            //throws System.IO.FileNotFoundException 
            sp.SourceFile = Path.Combine(inputPath, @"doesnotexist.zip");
            opt.DestinationFileExistsAction = UnzipFileExistAction.Overwrite;
            sp.DestinationDirectory = outputPath;
            Assert.That(() => ZipTask.ExtractArchive(sp, opt, new System.Threading.CancellationToken()), Throws.TypeOf<FileNotFoundException>());
        }

        [Test]
        public void DestinatioDirectoryNotFound()
        {
            //throws directory not found exception
            //destination directory does not exist and create destination directory == false
            sp.SourceFile = Path.Combine(inputPath, @"HiQLogos.zip");
            opt.DestinationFileExistsAction = UnzipFileExistAction.Error;
            opt.CreateDestinationDirectory = false;
            sp.DestinationDirectory = Path.Combine(outputPath, @"\doesnot\exist");
            Assert.That(() => ZipTask.ExtractArchive(sp, opt, new System.Threading.CancellationToken()), Throws.TypeOf<DirectoryNotFoundException>());
        }

        [Test]
        public void CreateDirectory()
        {
            //create destination directory if it does not exist
            sp.SourceFile = Path.Combine(inputPath, @"HiQLogos.zip");
            opt.DestinationFileExistsAction = UnzipFileExistAction.Error;
            opt.CreateDestinationDirectory = true;
            sp.DestinationDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\TestData\TestOut\new_directory\");

            outputFiles = new List<string>();
            fileNames.ToList().ForEach(x => outputFiles.Add(Path.Combine(sp.DestinationDirectory, x)));
            UnzipOutput output = ZipTask.ExtractArchive(sp, opt, new CancellationToken());

            foreach (string s in outputFiles)
            {
                Assert.True(File.Exists(s));
            }
            Assert.AreEqual(output.ExtractedFiles.Count, 7);
        }

        [Test]
        public void ExtractWithPassword()
        {
            //extract password protected archive
            sp.SourceFile = Path.Combine(inputPath, @"HiQLogosWithPassword.zip");
            sp.Password = "secret";

            opt.DestinationFileExistsAction = UnzipFileExistAction.Overwrite;
            opt.CreateDestinationDirectory = true;

            sp.DestinationDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\TestData\TestOut\new_directory");

            UnzipOutput output = ZipTask.ExtractArchive(sp, opt, new System.Threading.CancellationToken());
            Assert.True(File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\TestData\TestOut\new_directory\logo1.png")));
            Assert.True(File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\TestData\TestOut\new_directory\logo2.png")));
        }

        [Test]
        public void PasswordError()
        {
            //Should throw Ionic.Zip.BadPasswordException
            sp.SourceFile = Path.Combine(inputPath, @"HiQLogosWithPassword.zip");
            opt.DestinationFileExistsAction = UnzipFileExistAction.Overwrite;
            opt.CreateDestinationDirectory = true;
            sp.DestinationDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\TestData\TestOut\new_directory");

            Assert.That(() => ZipTask.ExtractArchive(sp, opt, new System.Threading.CancellationToken()), Throws.TypeOf<BadPasswordException>());
        }

        [Test]
        public void ThrowErrorOnOverwrite()
        {
            sp.SourceFile = Path.Combine(inputPath, @"HiQLogos.zip");

            UnzipOptions opt2 = new UnzipOptions()
            {
                DestinationFileExistsAction = UnzipFileExistAction.Overwrite,
                CreateDestinationDirectory = true
            };

            opt.DestinationFileExistsAction = UnzipFileExistAction.Error;
            opt.CreateDestinationDirectory = true;

            sp.DestinationDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\TestData\TestOut\new_directory");

            //unzip files to TestOut, so that there are existing files
            ZipTask.ExtractArchive(sp, opt2, new CancellationToken());

            Assert.That(() => ZipTask.ExtractArchive(sp, opt, new System.Threading.CancellationToken()), Throws.TypeOf<Ionic.Zip.ZipException>());
        }

        [Test]
        public void OverwriteFiles()
        {

            sp.SourceFile = Path.Combine(inputPath, @"testzip.zip");
            UnzipOptions opt = new UnzipOptions()
            {
                DestinationFileExistsAction = UnzipFileExistAction.Overwrite,
                CreateDestinationDirectory = true
            };

            sp.DestinationDirectory = Path.Combine(sp.DestinationDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\TestData\TestOut\new_directory"));

            //extract testzip.zip
            UnzipOutput output = ZipTask.ExtractArchive(sp, opt, new CancellationToken());
            //read first line from each file
            var lines = Directory.EnumerateFiles(sp.DestinationDirectory, "*", SearchOption.AllDirectories).Select(x => File.ReadLines(x).First()).ToList();

            Assert.True(lines.Contains("First file") && lines.Contains("Second file") && lines.Contains("Third file"));

            sp.SourceFile = Path.Combine(inputPath, @"testzip2.zip");
            //Extract testzip2.zip. Should overwrite existing files from previous step
            output = ZipTask.ExtractArchive(sp,  opt, new CancellationToken());
            var lines2 = Directory.EnumerateFiles(sp.DestinationDirectory, "*", SearchOption.AllDirectories).Select(x => File.ReadLines(x).First()).ToList();
            Assert.False(lines2.Contains("First file") && lines2.Contains("Second file") && lines2.Contains("Third file"));
            Assert.True(lines2.Contains("Fourth file") && lines2.Contains("Fifth file") && lines2.Contains("Sixth file"));
        }

        [Test]
        public void RenameFiles()
        {
            sp.SourceFile = Path.Combine(inputPath, @"HiQLogos.zip");

            opt.DestinationFileExistsAction = UnzipFileExistAction.Rename;
            opt.CreateDestinationDirectory = true;

            sp.DestinationDirectory = outputPath;

            //extract files to TestOut, so that there are existing files
            ZipTask.ExtractArchive(sp,  opt, new CancellationToken());
            UnzipOutput output = ZipTask.ExtractArchive(sp,  opt, new CancellationToken());
            //create filenames to test against
            outputFiles = new List<string>();
            renamedFilenames.ToList().ForEach(x => outputFiles.Add(Path.Combine(sp.DestinationDirectory, x)));

            foreach (string s in outputFiles)
            {
                Assert.True(File.Exists(s));

            }

            Assert.AreEqual(output.ExtractedFiles.Count, 7);
        }

    }

}