using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.IO;
using System.Windows.Threading;
using System.Diagnostics; // Process
using System.Net;
using System.Windows.Markup;

namespace compLexity_Demo_Player
{
    public partial class UpdateWindow : Window
    {
        private enum Status
        {
            Connecting,
            DownloadingUpdateProgram,
            Complete,
            Error
        }

        private readonly String[] statusText =
        {
            "Connecting...",
            "Downloading update program...",
            "Update program download complete. Click \"OK\" to continue.",
            ""
        };

        private readonly String[] buttonText = 
        {
            "Cancel",
            "Cancel",
            "OK",
            "Close"
        };

        private readonly String updateProgramFileName = "update.exe";
        private readonly String updateProgramConfigFileName = "update.exe.config";
        private readonly String changeLogFileName = "changelog.xml";

        private Status status;
        private Thread thread;

        public UpdateWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetStatus(Status.Connecting);

            thread = new Thread(new ThreadStart(ThreadWorker));
            thread.Start();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (status != Status.Complete && status != Status.Error)
            {
                Common.AbortThread(thread);
            }
        }

        private void uiInstallOrCancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();

            if (status == Status.Complete)
            {
                // run update.exe
                Process.Start(Config.Settings.ProgramPath + "\\" + updateProgramFileName);
                Application.Current.Shutdown();
            }
        }

        private void ThreadWorker()
        {
            // download changelog
            try
            {
                WebRequest request = WebRequest.Create(Config.Settings.UpdateUrl + changeLogFileName);

                using (WebResponse response = request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        // workaround for not being able to create a FlowDocument in a background thread
                        FlowDocument fd = (FlowDocument)XamlReader.Load(stream);
                        MemoryStream ms = new MemoryStream();
                        XamlWriter.Save(fd, ms);
                        ms.Position = 0;
                        SetChangeLogDocument(ms);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
                UpdateComplete(ex.Message);
                return;
            }

            // download update program
            SetStatus(Status.DownloadingUpdateProgram);
            String errorMessage = null;

            try
            {
                Common.WebDownloadBinaryFile(Config.Settings.UpdateUrl + updateProgramFileName, Config.Settings.ProgramPath + "\\" + updateProgramFileName, SetUpdateProgress);
                Common.WebDownloadBinaryFile(Config.Settings.UpdateUrl + updateProgramConfigFileName, Config.Settings.ProgramPath + "\\" + updateProgramConfigFileName, null);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
                errorMessage = ex.Message;
            }
            finally
            {
                UpdateComplete(errorMessage);
            }
        }

        private void SetStatus(Status newStatus)
        {
            Procedure<Status> action = s =>
            {
                status = s;
                uiStatusTextBlock.Text = statusText[(Int32)status];
                uiInstallOrCancelButton.Content = buttonText[(Int32)status];
            };

            if (Thread.CurrentThread != Dispatcher.Thread)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, action, newStatus);
            }
            else
            {
                action(newStatus);
            }
        }

        private void SetUpdateProgress(Int32 percent)
        {
            Procedure<Int32> action = p =>
            {
                if (uiStatusProgressBar.Value != p)
                {
                    uiStatusProgressBar.Value = p;
                }
            };

            if (Thread.CurrentThread != Dispatcher.Thread)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, action, percent);
            }
            else
            {
                action(percent);
            }
        }

        private void UpdateComplete(String errorMessage)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Procedure(() =>
            {
                if (errorMessage != null)
                {
                    SetUpdateProgress(0);
                    SetStatus(Status.Error);
                    uiStatusTextBlock.Text += " " + errorMessage;
                    return;
                }

                SetUpdateProgress(100);
                SetStatus(Status.Complete);
            }));
        }

        private void SetChangeLogDocument(MemoryStream stream)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Procedure(() =>
            {
                uiChangeLogFlowDocumentScrollViewer.Document = (FlowDocument)XamlReader.Load(stream);
            }));
        }
    }
}
