using System;
using System.Linq;
using FrameworkProcess = System.Diagnostics.Process;
using System.Collections.Generic;

namespace CDP.Core
{
    public interface IProcessFinder
    {
        IEnumerable<IProcess> FindByName(string name);
    }

    public class ProcessFinder : IProcessFinder
    {
        public IEnumerable<IProcess> FindByName(string name)
        {
            return FrameworkProcess.GetProcessesByName(name).Select(fwp => (IProcess)new Process(fwp));
        }
    }
}
