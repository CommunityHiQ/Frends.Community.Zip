using System;
using System.IO;
using System.Threading.Tasks;
//using FRENDS.Common.Files;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text;
using Frends.Community.Zip;

namespace FRENDS.Community.Tests
{
    [TestFixture]
    public class ZipTests
    {
        private static readonly string _basePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDi‌​rectory, "..\\..\\"));

        private readonly string _dirIn = Path.Combine(_basePath, @"TestFiles\In\");
        private readonly string _dirOut = Path.Combine(_basePath, @"TestFiles\Out\");
        
        [TearDown]
        public void TearDown()
        {
            DelIfExists(Path.Combine(_dirOut, "testi.zip"));
            DelIfExists(Path.Combine(_dirOut, "XmlTest1.xml"));
            DelIfExists(Path.Combine(_dirOut, "XmlTest2.xml"));
            DelIfExists(Path.Combine(_dirOut, "testi_w_password.zip"));
            DelIfExists(Path.Combine(_dirOut, "testi_recursive.zip"));
            DelIfExists(Path.Combine(_dirOut, @"recursivetest/XmlTest1.xml"));
            DelIfExists(Path.Combine(_dirOut, @"recursivetest/XmlTest2.xml"));
            DelIfExists(Path.Combine(_dirOut, "testi_flatfile.zip"));
            DelIfExists(Path.Combine(_dirOut, "testi_empty.zip"));

        }
        
        private static void DelIfExists(string p)
        {
            if (File.Exists(p))
            {
                File.Delete(p);
            }
        }

        [Test]
        public void ZipFiles()
        {
            //non-recursive test
            Zip.ZipInputParameters input = InputHelperZip(_dirIn, _dirOut, "*.xml", "testi.zip");
            Zip.ZipOutputInfo output = Zip.CreateArchive(input);
            StringAssert.AreEqualIgnoringCase("testi.zip", output.Filename);
            Assert.AreEqual(2, output.Filecount);
            Assert.That(File.Exists(_dirOut + "testi.zip"));
        }

        [Test]
        public void ZipFiles_recursive_test()
        {
            //Scan subdirectories
            Zip.ZipInputParameters input = InputHelperZip(_dirIn, _dirOut, "*.xml", "testi_recursive.zip", true);
            Zip.ZipOutputInfo output = Zip.CreateArchive(input);
            Assert.AreEqual(4, output.Filecount);
            Assert.That(File.Exists(_dirOut + "testi_recursive.zip"));
        }

        [Test]
        public void ZipFiles_create_flatfile()
        {
            //creates a flat file 
            Zip.ZipInputParameters input = InputHelperZip(_dirIn, _dirOut, "*.xml", "testi_flatfile.zip", true, true);
            Zip.ZipOutputInfo output = Zip.CreateArchive(input);
            Assert.AreEqual(4, output.Filecount);
            Assert.That(File.Exists(_dirOut + "testi_flatfile.zip"));

        }
        [Test]
        public void ZipFiles_empty_zip_error()
        {
            //trying to create an empty archive
            try
            {
                Zip.ZipInputParameters input = this.InputHelperZip(_dirIn, _dirOut, "*.abc", "testi_empty.zip", true, true,true);
                Zip.ZipOutputInfo output = Zip.CreateArchive(input);
            }
            catch(Exception e)
            {
            }
            Assert.That(!File.Exists(_dirOut + "testi_empty.zip"));
        }

        [Test]
        public void ZipFiles_destination_path_Fail()
        {
            // Try to use a non-existing directory as a destination
            try
            {
                Zip.ZipInputParameters input = InputHelperZip(_dirIn, _dirOut + @"/testi6/", "*.xml", "testi.zip");
                Zip.ZipOutputInfo output = Zip.CreateArchive(input);
            }
            catch (DirectoryNotFoundException ex)
            {
            }

            Assert.That(!File.Exists(_dirOut + "testi.zip"));
        }

        [Test]
        public void ZipFiles_source_path_Fail()
        {
            // Try to use a non-existing directory as a source
            try
            {
                Zip.ZipInputParameters input = InputHelperZip(_dirIn + @"/testi6/", _dirOut, "*.xml", "testi.zip");
                Zip.ZipOutputInfo output = Zip.CreateArchive(input);
            }
            catch (DirectoryNotFoundException ex)
            {
            }

            Assert.That(!File.Exists(_dirOut + "testi.zip"));
        }


        [Test]
        public void ZipFiles_WithPassword()
        {
            //create an archive using a password
            Zip.ZipInputParameters input = InputHelperZip(_dirIn, _dirOut, "*.xml", "testi_w_password.zip", false, false, false, "SalaKala3");
            Zip.ZipOutputInfo output = Zip.CreateArchive(input);

            StringAssert.AreEqualIgnoringCase("testi_w_password.zip", output.Filename);
            Assert.That(File.Exists(_dirOut + "testi_w_password.zip"));
        }

        //helper methods for Zip_v3 tests
        private Zip.ZipInputParameters InputHelperZip(String source, String destination, string mask, string filename, Boolean recursive = false, Boolean flat = false, Boolean error = true, string password = null)
        {
            Zip.ZipInputParameters input = new Zip.ZipInputParameters()
            {
                SourcePath = source,
                DestinationPath = destination,
                FileMask = mask,
                Filename = filename,
                ///optional parameters
                AddDirectories = recursive,
                CreateFlatfile = flat,
                EmptyZipError = error,
                Password = password
            };
            return input;
        }
    }
}

