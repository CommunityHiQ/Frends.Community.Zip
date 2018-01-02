using System;
using System.IO;
using Ionic.Zip;
using Frends.Tasks.Attributes;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

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
            [CustomDisplay(DisplayOption.Tab)]SourceProperties source, 
            [CustomDisplay(DisplayOption.Tab)]DestinationProperties destination, 
            [CustomDisplay(DisplayOption.Tab)]Options options,
            CancellationToken cancellationToken)
        {
            // validate that source and destination folders exist
            if (!Directory.Exists(source.DirectoryPath))
                throw new DirectoryNotFoundException($"Source directory {source.DirectoryPath} does not exist.");
            if (!Directory.Exists(destination.DirectoryPath) && !options.CreateDestinationFolder)
                throw new DirectoryNotFoundException($"Destination directory {destination.DirectoryPath} does not exist.");

            var sourceFiles = Directory.EnumerateFiles(source.DirectoryPath, source.FileMask, source.IncludeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            // If no files were found, throw error or return empty Output object
            if (sourceFiles.Count() == 0) {
                if (options.ThrowErrorIfNoFilesFound)
                    throw new FileNotFoundException($"No files found in {source.DirectoryPath} with file mask '{source.FileMask}'");
                else
                    return new Output { FileCount = 0 };
            }

            using (var zipFile = new ZipFile())
            {
                //if password is given add it to archive
                if (!string.IsNullOrWhiteSpace(destination.Password))
                    zipFile.Password = destination.Password;

                foreach(var fullPath in sourceFiles)
                {
                    // check if cancellation is requested
                    cancellationToken.ThrowIfCancellationRequested();

                    // Add all files to zip root
                    if (destination.FlattenFolders)
                    {
                        //check is file with same name alredy added
                        if (zipFile.ContainsEntry(Path.GetFileName(fullPath)))
                        {
                            if (destination.RenameDublicateFiles)
                                RenameAndAddFile(zipFile, fullPath);
                            else
                                throw new Exception($"File {fullPath} already exists in zip!");
                        }
                        else
                            zipFile.AddFile(fullPath, "");
                    }
                    else
                    {
                        zipFile.AddFile(fullPath, fullPath.GetRelativePath(source.DirectoryPath));
                    }
                }

                // check does destination directory exist and if it should be created
                if (options.CreateDestinationFolder && !Directory.Exists(destination.DirectoryPath))
                    Directory.CreateDirectory(destination.DirectoryPath);

                var destinationZipFileName = Path.Combine(destination.DirectoryPath, destination.FileName);

                if(File.Exists(destinationZipFileName))
                    switch (options.DestinationFileExistsAction)
                    {
                        case FileExistAction.Error:
                            throw new Exception($"Destination file {destinationZipFileName} already exists.");
                            break;
                        case FileExistAction.Rename:
                            destinationZipFileName = GetRenamedZipFileName(destinationZipFileName);
                            break;
                    }

                // save zip (overwites existing file)
                zipFile.Save(destinationZipFileName);

                return new Output { FileName = Path.GetFileName(destinationZipFileName), FilePath = destination.DirectoryPath, FileCount = zipFile.Count, ArchivedFiles = zipFile.EntryFileNames.ToList() };
            }
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

