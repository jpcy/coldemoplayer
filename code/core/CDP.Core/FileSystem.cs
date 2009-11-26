using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CDP.Core
{
    public interface IFileSystem
    {
        /// <summary>
        /// Open a binary file for reading.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>The opened file stream.</returns>
        FastFileStreamBase OpenRead(string fileName);

        /// <summary>
        /// Get the names of all files in the specified path that match the search pattern.
        /// </summary>
        /// <param name="path">The path to search for files in.</param>
        /// <param name="fileSearchPattern">The search pattern to determine which files to select.</param>
        /// <returns>An array of file names.</returns>
        string[] GetFiles(string path, string fileSearchPattern);

        /// <summary>
        /// Gets all the subfolders in the specified path.
        /// </summary>
        /// <param name="path">The path to search for subfolders in.</param>
        /// <returns>A list of folder names.</returns>
        IEnumerable<string> GetFolderNames(string path);

        /// <summary>
        /// Combine path elements to form a single absolute path.
        /// </summary>
        /// <param name="path">The absolute path to start with.</param>
        /// <param name="paths">Path elements to append to the starting path.</param>
        /// <returns>A combined path.</returns>
        string PathCombine(string path, params string[] paths);

        /// <summary>
        /// Gets a file name's extension without the '.'.
        /// </summary>
        /// <param name="fileName">The file name to retrieve the extension from.</param>
        /// <returns>A file extension without the '.'.</returns>
        string GetExtension(string fileName);

        /// <summary>
        /// Change a file name extension.
        /// </summary>
        /// <param name="fileName">The file name thats extension should be changed.</param>
        /// <param name="newExtension">The new extension.</param>
        /// <returns>A file name with the new extension.</returns>
        string ChangeExtension(string fileName, string newExtension);

        /// <summary>
        /// Gets a file name without it's extension.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>The specified file name without an extension.</returns>
        string GetFileNameWithoutExtension(string fileName);

        /// <summary>
        /// Determines whether a file exists.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>True if the file exists, otherwise false.</returns>
        bool FileExists(string fileName);

        /// <summary>
        /// Determines whether a directory exists.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <returns>True if the directory exists, otherwise false.</returns>
        bool DirectoryExists(string path);

        /// <summary>
        /// Create a directory at the specified path.
        /// </summary>
        /// <param name="path">The path to the directory to be created.</param>
        void CreateDirectory(string path);

        /// <summary>
        /// Deletes the directory at the specified path.
        /// </summary>
        /// <param name="path">The path to the directory to be deleted.</param>
        void DeleteDirectory(string path);

        /// <summary>
        /// Returns the directory for the specified path string.
        /// </summary>
        /// <param name="path">The path to retrieve the directory from.</param>
        /// <returns>The name of the directory.</returns>
        string GetDirectoryName(string path);

        /// <summary>
        /// Delete a file. An exception is not thrown if the file does not exist.
        /// </summary>
        /// <param name="fileName">The name of the file to delete.</param>
        void DeleteFile(string fileName);

        /// <summary>
        /// Calculate a random file name ensuring that a file with the same name doesn't exist in the specified path.
        /// </summary>
        /// <param name="path">The proposed path of the file.</param>
        /// <returns>A random, unique filename.</returns>
        string CalculateRandomFileName(string path);

        /// <summary>
        /// Gets the absolute path for the specified path string.
        /// </summary>
        /// <param name="path">The (possibly relative) path.</param>
        /// <returns>An absolute path.</returns>
        string GetFullPath(string path);
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

        public string GetFileNameWithoutExtension(string fileName)
        {
            return Path.GetFileNameWithoutExtension(fileName);
        }

        public bool FileExists(string fileName)
        {
            return File.Exists(fileName);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public string CalculateRandomFileName(string path)
        {
            string fileName;

            do
            {
                fileName = PathCombine(path, Path.GetRandomFileName());
            }
            while (FileExists(fileName));

            return fileName;
        }

        public void DeleteFile(string fileName)
        {
            // Documentation is misleading, File.Delete throws an exception if the *path* doesn't exist.
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }

        public void DeleteDirectory(string path)
        {
            Directory.Delete(path);
        }

        public string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }
    }
}
