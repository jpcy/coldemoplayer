using System;
using System.Linq;
using System.IO;

namespace CDP.Core
{
    public interface IFileSystem
    {
        Stream OpenRead(string fileName);
        string[] GetFiles(string path, string fileSearchPattern);
        string PathCombine(string path, params string[] paths);
        string GetExtension(string fileName);
    }

    public class FileSystem : IFileSystem
    {
        public Stream OpenRead(string fileName)
        {
            return File.OpenRead(fileName);
        }

        public string[] GetFiles(string path, string fileSearchPattern)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            return di.GetFiles(fileSearchPattern).Select(fi => fi.FullName).ToArray();
        }

        public string PathCombine(string path, params string[] paths)
        {
            foreach (string p in paths)
            {
                path = Path.Combine(path, p);
            }

            return path;
        }

        public string GetExtension(string fileName)
        {
            return Path.GetExtension(fileName).Replace(".", null);
        }
    }
}
