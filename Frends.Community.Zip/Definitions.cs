using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#pragma warning disable 1591 // Missing XML comment for publicly visible type or member

namespace Frends.Community.Zip
{
    public enum FileExistAction { Error, Overwrite, Rename};
    public enum UseZip64Option { Always, AsNecessary, Never };

    public class SourceProperties
    {
        /// <summary>
        /// Source directory
        /// </summary>
        [DefaultValue(@"C:\example\folder\")]
        [DisplayFormat(DataFormatString = "Text")]
        public string Directory { get; set; }

        /// <summary>
        /// The search string to match against the names of files. 
        /// This parameter can contain a combination of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions. 
        /// The default pattern is "*", which returns all files.
        /// </summary>
        [DefaultValue("*")]
        [DisplayFormat(DataFormatString = "Text")]
        public string FileMask { get; set; }

        /// <summary>
        /// Indicates if sub folders and files should also be zipped
        /// </summary>
        [DefaultValue(false)]
        public bool IncludeSubFolders { get; set; }
    }

    public class DestinationProperties
    {
        /// <summary>
        /// Destination directory
        /// </summary> 
        [DisplayFormat(DataFormatString = "Text")]
        public string Directory { get; set; }

        /// <summary>
        /// Filename of the zip to create
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
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
        /// True: If source files contains duplicate names, they are renamed (example.txt --&gt; example_(1).txt)
        /// False: Throws error if duplicate file names are found
        /// </summary>
        [UIHint(nameof(FlattenFolders), "", true)]
        [DefaultValue(false)]
        public bool RenameDuplicateFiles { get; set; }

    }

    public class Options
    {
        /// <summary>
        /// Always: Always use ZIP64 extensions when writing zip archives, even when unnecessary.
        /// AsNecessary: Use ZIP64 extensions when writing zip archives, as necessary (when single entry or entries in total exceeds 0xFFFFFFFF in size, or when there are more than 65535 entries)
        /// Never: Do not use ZIP64 extensions when writing zip archives.
        /// </summary>
        [DefaultValue(UseZip64Option.AsNecessary)]
        public UseZip64Option UseZip64 { get; set; }

        /// <summary>
        /// Throw error if no source files are found. Otherwise returns object with FileCount: 0
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
        /// Full path to zip created
        /// </summary>
        public string Path { get; set; }

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
