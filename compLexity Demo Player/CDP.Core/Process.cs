using System;
using FrameworkProcess = System.Diagnostics.Process;

namespace CDP.Core
{
    public interface IProcess
    {
        bool HasExited { get; }
        string FileName { get; }
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
                    result = process.MainModule.FileName;
                }
                catch (System.ComponentModel.Win32Exception)
                {
                }

                return result;
            }
        }
    }
}
