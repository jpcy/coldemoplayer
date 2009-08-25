using System;
using FrameworkAssembly = System.Reflection.Assembly;

namespace CDP.Core.Adapters
{
    public interface IAssembly
    {
        Type[] GetTypes(string assemblyFile);
    }

    public class Assembly : IAssembly
    {
        public Type[] GetTypes(string assemblyFile)
        {
            FrameworkAssembly assembly = FrameworkAssembly.LoadFile(assemblyFile);
            return assembly.GetTypes();
        }
    }
}
