using System;

namespace CDP.Core.Adapters
{
    public interface IFile
    {
        System.IO.Stream OpenRead(string path);
    }

    public class File : IFile
    {
        public System.IO.Stream OpenRead(string path)
        {
            return System.IO.File.OpenRead(path);
        }
    }
}
