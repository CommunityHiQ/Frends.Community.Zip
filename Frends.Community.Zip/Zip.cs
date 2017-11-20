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
        public class Source
        {
            /// <summary>
            /// Source path
            /// </summary>
            [DisplayName("Source path")]
            [DefaultValue("\"\\\"")]
            public string SourcePath { get; set; }

            [DisplayName("File mask")]
            [DefaultValue("\"*\"")]
            public string FileMask { get; set; }
        }

        /// <summary>
        /// The destination parameters of the class inputs
        /// </summary>
        public class Destination
        {
            /// <summary>
            /// Filename of the zip to create
            /// </summary>
            [DisplayName("Filename")]
            [DefaultValue("\"\"")]
            public string Filename { get; set; }

            /// <summary>
            /// Destination path
            /// </summary>
            [DisplayName("Destination path")]
            [DefaultValue("\"\\\"")]
            public string DestinationPath { get; set; }
            /// <summary>
            /// Filemask
            /// </summary>
        }

        public class Options
        {
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
        public class Output
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
        /// <param name="SourceParams">ZipInputParameters-object</param>
        /// <returns>ZipOutputInfo-object</returns>
        public static Output CreateArchive(Source SourceParams, Destination DestinationParams, Options OptionParams)
        {
            DirectoryInfo sourceDirInfo = new DirectoryInfo(SourceParams.SourcePath);
            DirectoryInfo destDirInfo = new DirectoryInfo(DestinationParams.DestinationPath);

            //adds a directory separator char
            SourceParams.DestinationPath = SourceParams.DestinationPath.TrimEnd(System.IO.Path.DirectorySeparatorChar) + @"\";

            if (!sourceDirInfo.Exists)
                throw new DirectoryNotFoundException("Source directory " + SourceParams.SourcePath + " not found");
            if (!destDirInfo.Exists)
                throw new DirectoryNotFoundException("Destination directory " + SourceParams.DestinationPath + " not found");
            if (string.IsNullOrEmpty(SourceParams.FileMask))
                throw new ArgumentNullException("File mask cannot be null");
            if (string.IsNullOrEmpty(SourceParams.Filename))
                throw new ArgumentNullException("Filename cannot be null");

            var outputInfo = new Output();

            using (ZipFile archive = new ZipFile())
            {
                //Adds a password to the archive if provided.
                //Password works only for files that are > 0 bytes
                if (!string.IsNullOrWhiteSpace(SourceParams.Password))
                {
                    archive.Password = SourceParams.Password;
                }
                //Creates a flat file
                if (SourceParams.AddDirectories && SourceParams.CreateFlatfile)
                {
                    foreach (FileInfo fileInfo in sourceDirInfo.EnumerateFiles(SourceParams.FileMask, SearchOption.AllDirectories))
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
                else if (SourceParams.AddDirectories && !SourceParams.CreateFlatfile)
                {
                    foreach (FileInfo fileInfo in sourceDirInfo.GetFiles(SourceParams.FileMask, SearchOption.AllDirectories))
                    {
                        //a stupid way to get the relative path
                        string relativePath = fileInfo.FullName.ToString().Replace(SourceParams.SourcePath, string.Empty).Replace(fileInfo.Name, string.Empty);
                        archive.AddFile(fileInfo.FullName, relativePath);
                    }
                }
                else
                {   //top directory only
                    foreach (FileInfo fileInfo in sourceDirInfo.EnumerateFiles(SourceParams.FileMask, SearchOption.TopDirectoryOnly))
                    {
                        archive.AddFile(fileInfo.FullName, "");
                    }
                }

                //if error flag is set
                if (archive.Count <= 0 && SourceParams.EmptyZipError)
                    throw new FileNotFoundException("No files found, check your source directory or file mask");

                //creates archive only when there are actual files in it
                if (archive.Count > 0)
                {
                    outputInfo.Filecount = archive.Count;
                    outputInfo.Filename = SourceParams.Filename;
                    //adds a directory separator char
                    archive.Save(SourceParams.DestinationPath + SourceParams.Filename);
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

