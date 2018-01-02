using Frends.Tasks.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

#pragma warning disable 1591 // Missing XML comment for publicly visible type or member

namespace Frends.Community.Zip
{
    public enum FileExistAction { Error, Overwrite, Rename};

    [DisplayName("Source")]
    public class SourceProperties
    {
        /// <summary>
        /// Source directory path
        /// </summary>
        [DefaultValue(@"C:\example\folder\")]
        [DefaultDisplayType(DisplayType.Text)]
        public string DirectoryPath { get; set; }

        /// <summary>
        /// The search string to match against the names of files. 
        /// This parameter can contain a combination of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions. 
        /// The default pattern is "*", which returns all files.
        /// </summary>
        [DefaultValue("*")]
        [DefaultDisplayType(DisplayType.Text)]
        public string FileMask { get; set; }

        /// <summary>
        /// Indicates if sub folders and files should also be zipped
        /// </summary>
        [DefaultValue(false)]
        public bool IncludeSubFolders { get; set; }
    }

    [DisplayName("Destination")]
    public class DestinationProperties
    {
        /// <summary>
        /// Destination directory path
        /// </summary>
        public string DirectoryPath { get; set; }

        /// <summary>
        /// Filename of the zip to create
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Add password protection to zip
        /// </summary>
        [PasswordPropertyText]
        public string Password { get; set; }


        /// <summary>
        /// Choose if source folder structure should be flatten.
        /// </summary>
        [DefaultValue(false)]
        public bool FlattenFolders { get; set; }

        /// <summary>
        /// True: If source files contains dublicate names, they are renamed (example.txt --&gt; example_(1).txt)
        /// False: Throws error if dublicate file names are found
        /// </summary>
        [ConditionalDisplay(nameof(FlattenFolders), true)]
        [DefaultValue(false)]
        public bool RenameDublicateFiles { get; set; }

    }

    [DisplayName("Options")]
    public class Options
    {
        /// <summary>
        /// Throw error if no source files are found
        /// </summary>
        [DefaultValue(true)]
        public bool ThrowErrorIfNoFilesFound { get; set; }

        /// <summary>
        /// Choose action if destination zip file already exists
        /// Error: throws error
        /// Overwrite: Overwrites existing zip file with new one
        /// Rename: Renames new zip file (example.zip --&gt; example_(1).zip)
        /// </summary>
        [DefaultValue(FileExistAction.Error)]
        public FileExistAction DestinationFileExistsAction { get; set; }

        /// <summary>
        /// Create destination folder if it does not exist
        /// </summary>
        [DefaultValue(false)]
        public bool CreateDestinationFolder { get; set; }
    }

    public class Output
    {
        /// <summary>
        /// Filename of zip created
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Path to zip created
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Number of files in creted zip file
        /// </summary>
        public Int32 FileCount { get; set; }

        /// <summary>
        /// List of files zipped
        /// </summary>
        public List<string> ArchivedFiles { get; set; }
    }
}
