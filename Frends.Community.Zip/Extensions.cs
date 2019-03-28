using System;
using System.IO;

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
    }
}
