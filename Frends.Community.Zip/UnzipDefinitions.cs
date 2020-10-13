using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Community.Zip
{

#pragma warning disable CS1591
    public enum UnzipFileExistAction { Error, Overwrite, Rename };
#pragma warning restore CS1591 
    /// <summary>
    /// Source properties
    /// </summary>
    [DisplayName("Input")]
    public class UnzipInputProperties
    {
        /// <summary>
        /// Full path to the source file
        /// </summary>
        [DefaultValue(@"C:\example\file.zip")]
        [DisplayName(@"Source file")]
        [DisplayFormat(DataFormatString="Text")]
        public string SourceFile { get; set; }
        /// <summary>
        /// Password for the zip file
        /// </summary>
        [DefaultValue("")]
        [PasswordPropertyText]
        public string Password { get; set; }
 
        /// <summary>
        /// Destination directory
        /// </summary>
        [DefaultValue(@"C:\example")]
        [DisplayName(@"Destination directory")]
        [DisplayFormat(DataFormatString ="Text")]
        public string DestinationDirectory { get; set; }
    }
    /// <summary>
    /// Options
    /// </summary>
    [DisplayName("Options")]
    public class UnzipOptions
    {
        /// <summary>
        /// Action to be taken when destination file/files exist
        /// </summary>
        [DefaultValue(FileExistAction.Error)]
        [DisplayName(@"File exist action")]
        public UnzipFileExistAction DestinationFileExistsAction { get; set; }
        /// <summary>
        /// Create destination directory if it does not exist
        /// </summary>
        [DefaultValue(false)]
        [DisplayName(@"Create destination directory")]
        public bool CreateDestinationDirectory { get; set; }
    }
    /// <summary>
    /// Output
    /// </summary>
    public class UnzipOutput
    {
        /// <summary>
        /// a List-object of extracted files
        /// </summary>
        public List<string> ExtractedFiles { get; set; }

#pragma warning disable CS1591
        public UnzipOutput()
#pragma warning restore CS1591
        {
            ExtractedFiles = new List<string>();
        }
    }
}
