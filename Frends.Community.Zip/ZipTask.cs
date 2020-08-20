using System;
using System.IO;
using Ionic.Zip;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

#pragma warning disable 1591 // Missing XML comment for publicly visible type or member

namespace Frends.Community.Zip
{
    public class ZipTask
    {
        /// <summary>
        /// Create zip archive. See https://github.com/CommunityHiQ/Frends.Community.Zip
        /// </summary>
        /// <returns>Object {string FileName, string FilePath, int FileCount, List&lt;string&gt; ArchivedFiles}</returns>
        public static Output CreateArchive(
            [PropertyTab]SourceProperties source,
            [PropertyTab]DestinationProperties destinationZip,
            [PropertyTab]Options options,
            CancellationToken cancellationToken)
        {
            // validate that source and destination folders exist
            if (!Directory.Exists(source.Directory) && source.SourceType == SourceFilesType.PathAndFileMask)
                throw new DirectoryNotFoundException($"Source directory {source.Directory} does not exist.");
            if (!Directory.Exists(destinationZip.Directory) && !options.CreateDestinationFolder)
                throw new DirectoryNotFoundException($"Destination directory {destinationZip.Directory} does not exist.");

            var sourceFiles = new List<string>();
            // Populate source files list according to input type
            switch(source.SourceType)
            {
                case SourceFilesType.PathAndFileMask:
                    sourceFiles = Directory.EnumerateFiles(source.Directory, source.FileMask, source.IncludeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
                    break;
                case SourceFilesType.FileList:
                    sourceFiles = source.FilePathsList;
                    break;
            }

            // If no files were found, throw error or return empty Output object
            if (sourceFiles.Count() == 0) {
                if (options.ThrowErrorIfNoFilesFound)
                    throw new FileNotFoundException($"No files found in {source.Directory} with file mask '{source.FileMask}'");
                else
                    return new Output { FileCount = 0 };
            }

            // check does destination directory exist and if it should be created
            if (options.CreateDestinationFolder && !Directory.Exists(destinationZip.Directory))
                Directory.CreateDirectory(destinationZip.Directory);

            var destinationZipFileName = Path.Combine(destinationZip.Directory, destinationZip.FileName);

            // check does destination zip exists
            bool destinationZipExists = File.Exists(destinationZipFileName);
            if (destinationZipExists)
                switch (options.DestinationFileExistsAction)
                {
                    case FileExistAction.Error:
                        throw new Exception($"Destination file {destinationZipFileName} already exists.");

                    case FileExistAction.Rename:
                        destinationZipFileName = GetRenamedZipFileName(destinationZipFileName);
                        break;
                }


            // Either create a new zip file or open existing one if Append was selected
            using (var zipFile = (destinationZipExists && options.DestinationFileExistsAction == FileExistAction.Append) ? ZipFile.Read(destinationZipFileName) : new ZipFile())
            {
                //Set 'UseZip64WhenSaving' - needed for large zip files
                zipFile.UseZip64WhenSaving = options.UseZip64.ConvertEnum<Zip64Option>();

                //if password is given add it to archive
                if (!string.IsNullOrWhiteSpace(destinationZip.Password))
                    zipFile.Password = destinationZip.Password;

                foreach(var fullPath in sourceFiles)
                {
                    // check if cancellation is requested
                    cancellationToken.ThrowIfCancellationRequested();

                    // FlattenFolders = true: add all files to zip root, otherwise adda folders to zip. 
                    // Only available when source type is path and filemask
                    var pathInArchive = (source.FlattenFolders || source.SourceType == SourceFilesType.FileList) ? "" : fullPath.GetRelativePath(source.Directory);
                    

                    AddFileToZip(zipFile, fullPath, pathInArchive, destinationZip.RenameDuplicateFiles);
                }



                // save zip (overwites existing file)
                zipFile.Save(destinationZipFileName);

                // remove source files?
                foreach(var fullPath in sourceFiles)
                    if (source.RemoveZippedFiles) File.Delete(fullPath);

                return new Output { Path = destinationZipFileName, FileCount = zipFile.Count, ArchivedFiles = zipFile.EntryFileNames.ToList() };
            }
        }

        private static void AddFileToZip(ZipFile zipFile, string fullPath, string pathInArchive, bool renameDublicateFile)
        {
            //check is file with same name alredy added
            if (zipFile.ContainsEntry(Path.GetFileName(fullPath)))
            {
                if (renameDublicateFile)
                    RenameAndAddFile(zipFile, fullPath);
                else
                    throw new Exception($"File {fullPath} already exists in zip!");
            }
            else
                zipFile.AddFile(fullPath, pathInArchive);
        }

        private static void RenameAndAddFile(ZipFile zipFile, string filePath)
        {
            var renamedFileName = GetRenamedFileName(zipFile.EntryFileNames, Path.GetFileName(filePath));
            zipFile.AddEntry(renamedFileName, File.ReadAllBytes(filePath));
        }

        private static string GetRenamedFileName(ICollection<string> existingFileNames, string fileName)
        {
            var index = 1;
            var renamedFile = fileName.RenameFile(index);
            while (existingFileNames.Contains(renamedFile))
            {
                index++;
                renamedFile = fileName.RenameFile(index);
            }

            return renamedFile;
        }

        private static string GetRenamedZipFileName(string fullPath)
        {
            var index = 1;
            var renamedFile = Path.GetFileName(fullPath).RenameFile(index);
            var path = Path.GetDirectoryName(fullPath);
            var renamedFileFullPath = Path.Combine(path, renamedFile);
            while (File.Exists(renamedFileFullPath))
            {
                index++;
                renamedFile = Path.GetFileName(fullPath).RenameFile(index);
                renamedFileFullPath = Path.Combine(path, renamedFile);
            }

            return renamedFileFullPath;
        }

        private static void Zip_ExtractProgress(object sender, ExtractProgressEventArgs e, UnzipOutput output, string fullPath)
        {
            if (e.EventType == ZipProgressEventType.Extracting_AfterExtractEntry && !e.CurrentEntry.IsDirectory)
            {
                if (e.ExtractLocation == null)
                {
                    //Path.GetFullPath changes directory separator to "\"
                    output.ExtractedFiles.Add(Path.GetFullPath(fullPath));
                }
                else
                {
                    output.ExtractedFiles.Add(Path.GetFullPath(Path.Combine(e.ExtractLocation, e.CurrentEntry.FileName)));
                }
            }
        }

        /// <summary>
        /// A Frends task for extracting zip archives. See https://github.com/CommunityHiQ/Frends.Community.Zip
        /// </summary>
        /// <param name="source">Source properties</param>
        /// <param name="destination">Destination properties</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Output-object with a List of extracted files</returns>
        public static UnzipOutput ExtractArchive(
            [PropertyTab] UnzipSourceProperties source, UnzipDestinationProperties destination,
            [PropertyTab] UnzipOptions options,
            CancellationToken cancellationToken)
        {

            if (!File.Exists(source.SourceFile))
                throw new FileNotFoundException($"Source file {source.SourceFile} does not exist.");

            if (!Directory.Exists(destination.DestinationDirectory) && !options.CreateDestinationDirectory)
                throw new DirectoryNotFoundException($"Destination directory {destination.DestinationDirectory} does not exist.");

            if (options.CreateDestinationDirectory)
            {
                Directory.CreateDirectory(destination.DestinationDirectory);
            }

            UnzipOutput output = new UnzipOutput();

            using (ZipFile zip = ZipFile.Read(source.SourceFile))
            {
                string path = null;
                zip.ExtractProgress += (sender, e) => Zip_ExtractProgress(sender, e, output, path);

                //if password is set
                if (!string.IsNullOrWhiteSpace(source.Password))
                {
                    zip.Password = source.Password;
                }

                switch (options.DestinationFileExistsAction)
                {
                    case UnzipFileExistAction.Error:
                    case UnzipFileExistAction.Overwrite:
                        zip.ExtractExistingFile = (options.DestinationFileExistsAction == UnzipFileExistAction.Overwrite) ? ExtractExistingFileAction.OverwriteSilently : ExtractExistingFileAction.Throw;
                        zip.ExtractAll(destination.DestinationDirectory);
                        break;
                    case UnzipFileExistAction.Rename:
                        foreach (ZipEntry z in zip)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            if (File.Exists(Path.Combine(destination.DestinationDirectory, z.FileName)))
                            {
                                //find a filename that does not exist 
                                string FullPath = Extensions.GetNewFilename(Path.Combine(Path.GetDirectoryName(destination.DestinationDirectory), z.FileName), z.FileName, cancellationToken);
                                path = FullPath;

                                using (FileStream fs = new FileStream(FullPath, FileMode.Create, FileAccess.Write))
                                {
                                    z.Extract(fs);
                                }
                            }
                            else
                            {
                                z.Extract(destination.DestinationDirectory);
                            }
                        }
                        break;
                }
            }
            return output;
        }
    }
}

