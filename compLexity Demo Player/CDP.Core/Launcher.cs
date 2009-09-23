using System;
using System.IO;
using System.Threading;

namespace CDP.Core
{
    public abstract class Launcher
    {
        public class ProcessFoundEventArgs : EventArgs
        {
            public IProcess Process { get; private set; }

            public ProcessFoundEventArgs(IProcess process)
            {
                Process = process;
            }
        }

        public event EventHandler<ProcessFoundEventArgs> ProcessFound;
        public event EventHandler ProcessClosed;
        public string Message { get; protected set; }

        public abstract string CalculateDestinationFileName();
        public abstract bool Verify();
        public abstract void Launch();

        private readonly IProcessFinder processFinder = ObjectCreator.Get<IProcessFinder>();
        protected string processExecutableFileName;
        private const int defaultMonitorProcessSleepTime = 250;

        public abstract void Initialise(Demo demo);

        public void MonitorProcessWorker()
        {
            MonitorProcessWorker(defaultMonitorProcessSleepTime);
        }

        public void MonitorProcessWorker(int sleepTime)
        {
            if (processExecutableFileName == null)
            {
                throw new InvalidOperationException("processExecutableFileName cannot be null.");
            }

            IProcess process = null;

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

                if (sleepTime > 0)
                {
                    Thread.Sleep(sleepTime);
                }
            }
        }

        private void OnProcessFound(IProcess process)
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

        protected IProcess FindProcess(string processName, string executableFileName)
        {
            var processes = processFinder.FindByName(processName);

            foreach (IProcess process in processes)
            {
                string compare = process.FileName ?? executableFileName;

                if (string.Equals(executableFileName, compare, StringComparison.CurrentCultureIgnoreCase))
                {
                    return process;
                }
            }

            return null;
        }
    }
}
