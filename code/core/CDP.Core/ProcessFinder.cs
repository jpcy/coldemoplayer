using System;
using System.Linq;
using FrameworkProcess = System.Diagnostics.Process;
using System.Collections.Generic;

namespace CDP.Core
{
    public interface IProcessFinder
    {
        /// <summary>
        /// Find all processes with the given name.
        /// </summary>
        /// <param name="name">The name of the process to find.</param>
        /// <returns>A list of processes that match the given name.</returns>
        IEnumerable<IProcess> FindByName(string name);

        /// <summary>
        /// Find all processes with the given name, excluding any processes with a certain ID.
        /// </summary>
        /// <param name="name">The name of the process to find.</param>
        /// <param name="excludeProcessId">The ID of the process to exclude from the search.</param>
        /// <returns>A list of processes that match the given name, exluding processes with the specified ID.</returns>
        IEnumerable<IProcess> FindByName(string name, int excludeProcessId);

        /// <summary>
        /// Find all processes with the given name and filename, excluding any processes with a certain ID.
        /// </summary>
        /// <param name="name">The name of the process to find.</param>
        /// <param name="fileName">The absolute filename of the process executable.</param>
        /// <param name="excludeProcessId">The ID of the process to exclude from the search.</param>
        /// <returns>A list of processes that match the given name and filename, exluding processes with the specified ID.</returns>
        IEnumerable<IProcess> FindByName(string name, string fileName, int excludeProcessId);

        /// <summary>
        /// Gets the current running process.
        /// </summary>
        /// <returns>The current running process.</returns>
        IProcess GetCurrentProcess();
    }

    public class ProcessFinder : IProcessFinder
    {
        public IEnumerable<IProcess> FindByName(string name)
        {
            return FrameworkProcess.GetProcessesByName(name).Select(fwp => (IProcess)new Process(fwp));
        }

        public IEnumerable<IProcess> FindByName(string name, int excludeProcessId)
        {
            return FrameworkProcess.GetProcessesByName(name).Where(fwp => fwp.Id != excludeProcessId).Select(fwp => (IProcess)new Process(fwp));
        }

        public IEnumerable<IProcess> FindByName(string name, string fileName, int excludeProcessId)
        {
            IEnumerable<IProcess> processes = FrameworkProcess.GetProcessesByName(name).Where(fwp => fwp.Id != excludeProcessId).Select(fwp => (IProcess)new Process(fwp));

            return processes.Where(p => p.FileName == fileName);
        }

        public IProcess GetCurrentProcess()
        {
            return (IProcess)new Process(FrameworkProcess.GetCurrentProcess());
        }
    }
}
