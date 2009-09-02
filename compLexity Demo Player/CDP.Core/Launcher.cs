using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace CDP.Core
{
    public abstract class Launcher
    {
        public class ProcessFoundEventArgs : EventArgs
        {
            public Process Process { get; private set; }

            public ProcessFoundEventArgs(Process process)
            {
                Process = process;
            }
        }

        public event EventHandler<ProcessFoundEventArgs> ProcessFound;
        public event EventHandler ProcessClosed;
        public string Message { get; protected set; }

        public abstract bool Verify();
        public abstract void Launch();

        protected string processExecutableFileName;
        private const int monitorProcessSleepTime = 250;

        public void MonitorProcessWorker()
        {
            if (processExecutableFileName == null)
            {
                throw new InvalidOperationException("processExecutableFileName cannot be null.");
            }

            Process process = null;

            while (true)
            {
                if (process == null)
                {
                    process = FindProcess(Path.GetFileNameWithoutExtension(processExecutableFileName), processExecutableFileName);

                    if (process != null)
                    {
                        OnProcessFound(process);
                    }
                }
                else
                {
                    if (process.HasExited)
                    {
                        OnProcessClosed();
                        return;
                    }
                }

                Thread.Sleep(monitorProcessSleepTime);
            }
        }

        private void OnProcessFound(Process process)
        {
            if (ProcessFound != null)
            {
                ProcessFound(this, new ProcessFoundEventArgs(process));
            }
        }

        private void OnProcessClosed()
        {
            if (ProcessClosed != null)
            {
                ProcessClosed(this, EventArgs.Empty);
            }
        }

        protected Process FindProcess(string processName, string executableFileName)
        {
            Process[] processes = Process.GetProcessesByName(processName);

            foreach (Process process in processes)
            {
                string compare = executableFileName;

                try
                {
                    compare = process.MainModule.FileName;
                }
                catch (System.ComponentModel.Win32Exception)
                {
                }

                if (string.Equals(executableFileName, compare, StringComparison.CurrentCultureIgnoreCase))
                {
                    return process;
                }
            }

            return null;
        }
    }
}
