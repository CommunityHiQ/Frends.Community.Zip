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
            if (!Directory.Exists(source.Directory))
                throw new DirectoryNotFoundException($"Source directory {source.Directory} does not exist.");
            if (!Directory.Exists(destinationZip.Directory) && !options.CreateDestinationFolder)
                throw new DirectoryNotFoundException($"Destination directory {destinationZip.Directory} does not exist.");

            var sourceFiles = Directory.EnumerateFiles(source.Directory, source.FileMask, source.IncludeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            // If no files were found, throw error or return empty Output object
            if (sourceFiles.Count() == 0) {
                if (options.ThrowErrorIfNoFilesFound)
                    throw new FileNotFoundException($"No files found in {source.Directory} with file mask '{source.FileMask}'");
                else
                    return new Output { FileCount = 0 };
            }

            using (var zipFile = new ZipFile())
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

                    // Add all files to zip root
                    if (destinationZip.FlattenFolders)
                    {
                        //check is file with same name alredy added
                        if (zipFile.ContainsEntry(Path.GetFileName(fullPath)))
                        {
                            if (destinationZip.RenameDuplicateFiles)
                                RenameAndAddFile(zipFile, fullPath);
                            else
                                throw new Exception($"File {fullPath} already exists in zip!");
                        }
                        else
                            zipFile.AddFile(fullPath, "");
                    }
                    else
                    {
                        zipFile.AddFile(fullPath, fullPath.GetRelativePath(source.Directory));
                    }
                }

                // check does destination directory exist and if it should be created
                if (options.CreateDestinationFolder && !Directory.Exists(destinationZip.Directory))
                    Directory.CreateDirectory(destinationZip.Directory);

                var destinationZipFileName = Path.Combine(destinationZip.Directory, destinationZip.FileName);

                if(File.Exists(destinationZipFileName))
                    switch (options.DestinationFileExistsAction)
                    {
                        case FileExistAction.Error:
                            throw new Exception($"Destination file {destinationZipFileName} already exists.");
                            
                        case FileExistAction.Rename:
                            destinationZipFileName = GetRenamedZipFileName(destinationZipFileName);
                            break;
                    }

                // save zip (overwites existing file)
                zipFile.Save(destinationZipFileName);

                // remove source files?
                foreach(var fullPath in sourceFiles)
                    if (source.RemoveZippedFiles) File.Delete(fullPath);

                return new Output { Path = destinationZipFileName, FileCount = zipFile.Count, ArchivedFiles = zipFile.EntryFileNames.ToList() };
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

