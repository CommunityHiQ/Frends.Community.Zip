using System;
using System.IO;
using Ionic.Zip;
using Frends.Tasks.Attributes;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Frends.Community.Zip
{
    public class Zip
    {
        public static Output CreateArchive([CustomDisplay(DisplayOption.Tab)]SourceProperties sourceProperties, 
            [CustomDisplay(DisplayOption.Tab)]DestinationProperties destinationProperties, 
            [CustomDisplay(DisplayOption.Tab)]Options options,
            CancellationToken cancellationToken)
        {
            var destination = destinationProperties.Destination;
            var destinationOptions = destinationProperties.Options;

            // validate that source and destination folders exist
            if (!Directory.Exists(sourceProperties.Path))
                throw new DirectoryNotFoundException($"Source directory {sourceProperties.Path} does not exist.");
            if (!Directory.Exists(destination.Path) && !destination.CreateDestinationFolder)
                throw new DirectoryNotFoundException($"Destination directory {destination.Path} does not exist.");

            var sourceFiles = Directory.EnumerateFiles(sourceProperties.Path, sourceProperties.FileMask, sourceProperties.IncludeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            // If no files were found, throw error or return empty Output object
            if (sourceFiles.Count() == 0) {
                if (options.ThrowErrorIfNoFilesFound)
                    throw new FileNotFoundException($"No files found in {sourceProperties.Path} with file mask '{sourceProperties.FileMask}'");
                else
                    return new Output { FileCount = 0 };
            }

            using (var zipFile = new ZipFile())
            {
                //if password is given add it to archive
                if (!string.IsNullOrWhiteSpace(destinationOptions.Password))
                    zipFile.Password = destinationOptions.Password;


                foreach(var fullPath in Directory.EnumerateFiles(
                    sourceProperties.Path, sourceProperties.FileMask, 
                    sourceProperties.IncludeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    // Add all files to zip root
                    if (destinationOptions.FlattenFolders)
                    {
                        //check is file with same name alredy added
                        if (zipFile.ContainsEntry(Path.GetFileName(fullPath)))
                        {
                            if (destinationOptions.RenameDublicateFiles)
                                RenameAndAddFile(zipFile, fullPath);
                            else
                                throw new Exception($"File {fullPath} already exists in zip!");
                        }
                        else
                            zipFile.AddFile(fullPath, "");
                    }
                    else
                    {
                        // get relative path to file
                        var relativePath = Path.GetDirectoryName(fullPath).Replace(Path.GetDirectoryName(sourceProperties.Path), string.Empty);
                        zipFile.AddFile(fullPath, relativePath);
                    }
                }

                // check does destination directory exist and if it should be created
                if (destination.CreateDestinationFolder && !Directory.Exists(destination.Path))
                    Directory.CreateDirectory(destination.Path);

                // save zip
                zipFile.Save(Path.Combine(destination.Path, destination.FileName));

                return new Output { Name = destination.FileName, Path = destination.Path, FileCount = zipFile.Count, FileNames = zipFile.EntryFileNames.ToList() };
            }
        }

        private static void RenameAndAddFile(ZipFile zipFile, string filePath)
        {
            var renamedFileName = GetRenamedFileName(zipFile.EntryFileNames, Path.GetFileName(filePath));
            zipFile.AddEntry(renamedFileName, File.ReadAllBytes(filePath));
        }

        private static string GetRenamedFileName(ICollection<string> entryFileNames, string fileName)
        {
            var index = 1;
            var renamedFile = fileName.RenameFile(index);
            while (entryFileNames.Contains(renamedFile))
            {
                index++;
                renamedFile = fileName.RenameFile(index);
            }

            return renamedFile;
        }
    }
}

