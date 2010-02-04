using System;
using FrameworkProcess = System.Diagnostics.Process;
using CDP.Core.Extensions;

namespace CDP.Core
{
    public interface IProcess
    {
        bool HasExited { get; }
        string FileName { get; }
        int Id { get; }
        IntPtr MainWindowHandle { get; }
        string Name { get; }
    }

    public class Process : IProcess
    {
        private readonly FrameworkProcess process;

        public Process(FrameworkProcess process)
        {
            this.process = process;
        }

        public bool HasExited
        {
            get { return process.HasExited; }
        }

        public string FileName
        {
            get
            {
                string result = null;

                try
                {
                    int attemptsLeft = 10;

                    while (process.MainModule == null)
                    {
                        if (attemptsLeft == 0)
                        {
                            throw new ApplicationException("Process MainModule still null after {0} attempts.".Args(attemptsLeft));
                        }

                        System.Threading.Thread.Sleep(100);
                        attemptsLeft--;
                    }

                    result = process.MainModule.FileName;
                }
                catch (System.ComponentModel.Win32Exception)
                {
                }

                return result;
            }
        }

        public int Id
        {
            get { return process.Id; }
        }

        public IntPtr MainWindowHandle
        {
            get { return process.MainWindowHandle; }
        }

        public string Name
        {
            get { return process.ProcessName; }
        }
    }
}
