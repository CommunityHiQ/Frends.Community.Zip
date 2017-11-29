using System.IO;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Frends.Community.Zip
{
    public static class Extensions
    {
        public static string RenameFile(this string fileName, int index)
        {
            return $"{Path.GetFileNameWithoutExtension(fileName)}_({index}){Path.GetExtension(fileName)}";
        }
    }
}
