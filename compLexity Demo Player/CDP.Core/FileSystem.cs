using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace CDP.Core
{
    public interface IFileSystem
    {
        FastFileStreamBase OpenRead(string fileName);
        string[] GetFiles(string path, string fileSearchPattern);
        IEnumerable<string> GetFolderNames(string path);
        string PathCombine(string path, params string[] paths);
        string GetExtension(string fileName);
    }

    public class FileSystem : IFileSystem
    {
        public FastFileStreamBase OpenRead(string fileName)
        {
            return FastFileStream.Open(fileName, FastFileAccess.Read);
        }

        public string[] GetFiles(string path, string fileSearchPattern)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            return di.GetFiles(fileSearchPattern).Select(fi => fi.FullName).ToArray();
        }

        public IEnumerable<string> GetFolderNames(string path)
        {
            DirectoryInfo root = new DirectoryInfo(path);
            return root.GetDirectories().Select(di => di.Name);
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
