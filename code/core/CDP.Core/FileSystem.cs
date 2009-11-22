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
        string ChangeExtension(string fileName, string newExtension);
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

        public string ChangeExtension(string fileName, string newExtension)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }
            if (string.IsNullOrEmpty(newExtension))
            {
                throw new ArgumentNullException("newExtension");
            }

            // Add leading '.' to extension.
            if (!newExtension.StartsWith("."))
            {
                newExtension = "." + newExtension;
            }

            return Path.GetFileNameWithoutExtension(fileName) + newExtension;
        }
    }
}
