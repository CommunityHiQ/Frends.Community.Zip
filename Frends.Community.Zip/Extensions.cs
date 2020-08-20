using System;
using System.IO;
using System.Threading;

#pragma warning disable 1591 // Missing XML comment for publicly visible type or member

namespace Frends.Community.Zip
{
    static class Extensions
    {
        //Converts  enum to requested enum type
        public static TEnum ConvertEnum<TEnum>(this Enum source)
        {
            return (TEnum)Enum.Parse(typeof(TEnum), source.ToString(), true);
        }

        public static string RenameFile(this string fileName, int index)
        {
            return $"{Path.GetFileNameWithoutExtension(fileName)}_({index}){Path.GetExtension(fileName)}";
        }

        public static string GetRelativePath(this string fullPath, string baseDirectory)
        {
            baseDirectory = baseDirectory.EndsWith(@"\") ? baseDirectory : $"{baseDirectory}\\";
            return Path.GetDirectoryName(fullPath).Replace(Path.GetDirectoryName(baseDirectory), string.Empty);
        }

        internal static string GetNewFilename(string fullPath, string name, CancellationToken cancellationToken)
        {
            int index = 0;
            string newPath = null;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                string new_Filename = $"{Path.GetFileNameWithoutExtension(name)}({index}){Path.GetExtension(name)}";
                newPath = Path.Combine(Path.GetDirectoryName(fullPath), new_Filename);
                index++;
            } while (File.Exists(newPath));
            return newPath;
        }
    }
}
