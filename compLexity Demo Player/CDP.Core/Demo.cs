using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace CDP.Core
{
    public abstract class Demo
    {
        public class ProgressChangedEventArgs : EventArgs
        {
            public int Progress { get; private set; }

            public ProgressChangedEventArgs(int progress)
            {
                Progress = progress;
            }
        }

        public class OperationErrorEventArgs : EventArgs
        {
            private readonly Exception exception;
            private readonly string errorMessage;

            public Exception Exception
            {
                get { return exception; }
            }

            public string ErrorMessage
            {
                get { return errorMessage ?? exception.Message; }
            }

            public OperationErrorEventArgs(string errorMessage)
                : this(errorMessage, null)
            {
            }

            public OperationErrorEventArgs(Exception exception)
                : this(null, exception)
            {
            }

            public OperationErrorEventArgs(string errorMessage, Exception exception)
            {
                this.errorMessage = errorMessage;
                this.exception = exception;
            }
        }

        public class Detail
        {
            public string Name { get; set; }
            public object Value { get; set; }
        }

        public event EventHandler<ProgressChangedEventArgs> ProgressChangedEvent;
        public event EventHandler<OperationErrorEventArgs> OperationErrorEvent;
        public event EventHandler OperationCompleteEvent;

        public virtual DemoHandler Handler { get; set; }
        public string Name { get; private set; }

        private string fileName = null;
        public string FileName 
        {
            get { return fileName; }
            set
            {
                if (fileName == null)
                {
                    fileName = value;
                    Name = Path.GetFileNameWithoutExtension(fileName);
                }
                else
                {
                    throw new InvalidOperationException("Filename is already set and cannot be changed.");
                }
            }
        }

        public abstract string GameName { get; }
        public abstract string MapName { get; protected set; }
        public abstract string Perspective { get; protected set; }
        public abstract TimeSpan Duration { get; protected set; }
        public abstract IList<Detail> Details { get; protected set; }
        public abstract ArrayList Players { get; protected set; }

        /// <summary>
        /// File locations to try and load an icon from. Filenames are tried in order. If no icon can be found, a generic "unknown" icon is used.
        /// </summary>
        public abstract string[] IconFileNames { get; }

        /// <summary>
        /// The relative path to the map preview and overview images corresponding to this demo.
        /// </summary>
        /// <example>goldsrc\de_dust2.jpg</example>
        public abstract string MapImagesRelativePath { get; }

        // Capabilities.
        public abstract bool CanPlay { get; }
        public abstract bool CanAnalyse { get; }

        // Operations.
        public abstract void Load();
        public abstract void Read();
        public abstract void Write(string destinationFileName);

        protected void OnProgressChanged(int progress)
        {
            if (ProgressChangedEvent != null)
            {
                ProgressChangedEvent(this, new ProgressChangedEventArgs(progress));
            }
        }

        protected void OnOperationError(string errorMessage, Exception exception)
        {
            if (OperationErrorEvent != null)
            {
                OperationErrorEvent(this, new OperationErrorEventArgs(errorMessage, exception));
            }
        }

        protected void OnOperationComplete()
        {
            if (OperationCompleteEvent != null)
            {
                OperationCompleteEvent(this, EventArgs.Empty);
            }
        }
    }
}
