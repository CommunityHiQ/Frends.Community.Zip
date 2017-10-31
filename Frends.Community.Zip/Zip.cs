using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Ionic.Zip;
using System.ComponentModel;

namespace Frends.Community.Zip
{
    /// <summary>
    /// A FRENDS task for creating and extracting zip archives v3
    /// </summary>
    public class Zip
    {
        /// <summary>
        /// Zip input parameters class
        /// </summary>
        public class ZipInputParameters
        {
            /// <summary>
            /// Filename
            /// </summary>
            [DisplayName("Filename")]
            [DefaultValue("\"\"")]
            public string Filename { get; set; }
            /// <summary>
            /// Source path
            /// </summary>
            [DisplayName("Source path")]
            [DefaultValue("\"\\\"")]
            public string SourcePath { get; set; }
            /// <summary>
            /// Destination path
            /// </summary>
            [DisplayName("Destination path")]
            [DefaultValue("\"\\\"")]
            public string DestinationPath { get; set; }
            /// <summary>
            /// Filemask
            /// </summary>
            [DisplayName("File mask")]
            [DefaultValue("\"*\"")]
            public string FileMask { get; set; }
            /// <summary>
            /// Recursive scan
            /// </summary>
            [DisplayName("Add directories ?")]
            [DefaultValue(true)]
            public bool AddDirectories { get; set; }
            /// <summary>
            /// Create a flat file archive
            /// </summary>
            [DisplayName("Create a flat archive ?")]
            [DefaultValue(false)]
            public bool CreateFlatfile { get; set; }
            /// <summary>
            /// Adds a password to archive
            /// </summary>
            [PasswordPropertyText]
            public string Password { get; set; }
            /// <summary>
            /// If set to true, will throw an error
            /// when task tries to create an empty archive
            /// </summary>
            [DisplayName("Throw error when zip is empty")]
            [DefaultValue(true)]
            public bool EmptyZipError { get; set; }
        }
        /// <summary>
        /// zip output info class
        /// </summary>
        public class ZipOutputInfo
        {
            /// <summary>
            /// Filename
            /// </summary>
            public string Filename { get; set; }
            /// <summary>
            /// Filecount
            /// </summary>
            [DefaultValue(0)]
            public Int32 Filecount { get; set; }
        }
        /// <summary>
        /// A FRENDS task for creating zip-archives. 
        /// </summary>
        /// <param name="inParameters">ZipInputParameters-object</param>
        /// <returns>ZipOutputInfo-object</returns>
        public static ZipOutputInfo CreateArchive(ZipInputParameters inParameters)
        {
            DirectoryInfo sourceDirInfo = new DirectoryInfo(inParameters.SourcePath);
            DirectoryInfo destDirInfo = new DirectoryInfo(inParameters.DestinationPath);

            //adds a directory separator char
            inParameters.DestinationPath = inParameters.DestinationPath.TrimEnd(System.IO.Path.DirectorySeparatorChar) + @"\";

            if (!sourceDirInfo.Exists)
                throw new DirectoryNotFoundException("Source directory " + inParameters.SourcePath + " not found");
            if (!destDirInfo.Exists)
                throw new DirectoryNotFoundException("Destination directory " + inParameters.DestinationPath + " not found");
            if (string.IsNullOrEmpty(inParameters.FileMask))
                throw new ArgumentNullException("File mask cannot be null");
            if (string.IsNullOrEmpty(inParameters.Filename))
                throw new ArgumentNullException("Filename cannot be null");

            var outputInfo = new ZipOutputInfo();

            using (ZipFile archive = new ZipFile())
            {
                //Adds a password to the archive if provided.
                //Password works only for files that are > 0 bytes
                if (!string.IsNullOrWhiteSpace(inParameters.Password))
                {
                    archive.Password = inParameters.Password;
                }
                //Creates a flat file
                if (inParameters.AddDirectories && inParameters.CreateFlatfile)
                {
                    foreach (FileInfo fileInfo in sourceDirInfo.EnumerateFiles(inParameters.FileMask, SearchOption.AllDirectories))
                    {
                        //If archive already contains a file with the same name
                        if (archive.ContainsEntry(fileInfo.Name))
                        {
                            using (FileStream fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
                            {
                                //to get around the ionic-zip <filename, filepath>-dictionary limitation (you can't have two files with the same name)
                                byte[] bt = new byte[fs.Length];
                                fs.Read(bt, 0, (int)fs.Length);
                                MemoryStream ms = new MemoryStream(bt, writable: false);
                                string new_filename;

                                //if file doesn't have a file extension
                                if (String.IsNullOrEmpty(fileInfo.Extension))
                                {                                       //creates an unique filename: filename + timestamp
                                    new_filename = fileInfo.Name + $"({GetTimestamp(DateTime.Now)})";
                                }
                                else
                                {
                                    //if file has a file extension
                                    new_filename = fileInfo.Name.Replace(fileInfo.Extension, String.Empty) +
                                        $"({GetTimestamp(DateTime.Now)}){fileInfo.Extension}";
                                }
                                archive.AddEntry(new_filename, ms);
                            }
                        }
                        else
                        {
                            archive.AddFile(fileInfo.FullName, "");
                        }
                    }
                    //include subdirectories in the archive
                }
                else if (inParameters.AddDirectories && !inParameters.CreateFlatfile)
                {
                    foreach (FileInfo fileInfo in sourceDirInfo.GetFiles(inParameters.FileMask, SearchOption.AllDirectories))
                    {
                        //a stupid way to get the relative path
                        string relativePath = fileInfo.FullName.ToString().Replace(inParameters.SourcePath, string.Empty).Replace(fileInfo.Name, string.Empty);
                        archive.AddFile(fileInfo.FullName, relativePath);
                    }
                }
                else
                {   //top directory only
                    foreach (FileInfo fileInfo in sourceDirInfo.EnumerateFiles(inParameters.FileMask, SearchOption.TopDirectoryOnly))
                    {
                        archive.AddFile(fileInfo.FullName, "");
                    }
                }

                //if error flag is set
                if (archive.Count <= 0 && inParameters.EmptyZipError)
                    throw new FileNotFoundException("No files found, check your source directory or file mask");

                //creates archive only when there are actual files in it
                if (archive.Count > 0)
                {
                    outputInfo.Filecount = archive.Count;
                    outputInfo.Filename = inParameters.Filename;
                    //adds a directory separator char
                    archive.Save(inParameters.DestinationPath + inParameters.Filename);
                }
            }
            return outputInfo;
        }     
        //helper-method for creating a "timestamp"
        private static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }
    }
}

