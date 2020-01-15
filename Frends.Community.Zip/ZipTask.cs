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

                    // FlattenFolders = true: add all files to zip root, otherwise adda folders to zip
                    var pathInArchive = destinationZip.FlattenFolders ? "" : fullPath.GetRelativePath(source.Directory);

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
            var renamedFile = Path.GetFileName(fullPath).RenameFile(index); ;
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
    }
}

