using System;
using FrameworkPath = System.IO.Path;

namespace CDP.Core.Adapters
{
    public interface IPath
    {
        string Combine(string path, params string[] paths);
        string GetExtension(string path);
    }

    public class Path : IPath
    {
        public string Combine(string path, params string[] paths)
        {
            foreach (string p in paths)
            {
                path = FrameworkPath.Combine(path, p);
            }

            return path;
        }

        public string GetExtension(string path)
        {
            return FrameworkPath.GetExtension(path).Replace(".", null);
        }
    }
}
