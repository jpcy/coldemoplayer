using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace compLexity_Demo_Player
{
    public interface IProgressWindow
    {
        void SetTitle(String title);
        void UpdateProgress(Int32 percent);
        void CloseWithResult(Boolean result);
        void Error(String errorMessage, Exception ex, Boolean continueAbort, Procedure<MessageWindow.Result> setResult);
    }

    public partial class ProgressWindow : IProgressWindow
    {
        /// <summary>
        /// The window is being closed by an external source (i.e. CloseWindow was called).
        /// </summary>
        protected Boolean closingWindowExternal = false;

        public void SetTitle(String title)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Procedure(() =>
            {
                this.Title = title;
            }));
        }

        public void UpdateProgress(Int32 percent)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Procedure(() =>
            {
                if (uiProgressBar.Value != percent)
                {
                    uiProgressBar.Value = percent;
                }
            }));
        }

        public void CloseWithResult(Boolean result)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Procedure(() =>
            {
                closingWindowExternal = true;
                DialogResult = result;
                Close();
            }));
        }

        /// <summary>
        /// Purpose: so background threads can correctly display a modal error window.
        /// </summary>
        public void Error(String errorMessage, Exception ex, Boolean continueAbort, Procedure<MessageWindow.Result> setResult)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Procedure(() =>
            {
                MessageWindow.Flags flags = MessageWindow.Flags.Error;

                if (continueAbort)
                {
                    flags |= MessageWindow.Flags.ContinueAbort;
                }

                Common.Message(this, errorMessage, ex, flags, setResult);
            }));
        }
    }
}
