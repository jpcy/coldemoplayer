using System;
using System.Linq;
using DirectoryInfo = System.IO.DirectoryInfo;
using FileInfo = System.IO.FileInfo;

namespace CDP.Core.Adapters
{
    public interface IFolder
    {
        string[] GetFiles(string path, string fileSearchPattern);
    }

    public class Folder : IFolder
    {
        public string[] GetFiles(string path, string fileSearchPattern)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            return di.GetFiles(fileSearchPattern).Select(fi => fi.FullName).ToArray();
        }
    }
}
