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
using System.Windows.Navigation;
using System.Threading;
using System.Windows.Threading;

namespace Update
{
    public partial class MainWindow : Window
    {
        private Boolean hasActivated = false;
        private Thread thread;
        private Boolean hasPassedThePointOfNoReturn = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (hasActivated)
            {
                return;
            }

            hasActivated = true;

            Updater updater = new Updater(AppendToLog, PointOfNoReturn, UpdateComplete);
            thread = new Thread(new ThreadStart(updater.ThreadStart));
            thread.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // don't allow closing past "the point of no return" - when files have begun downloading
            if (hasPassedThePointOfNoReturn)
            {
                e.Cancel = true;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (thread != null && thread.IsAlive)
            {
                thread.Abort();
                thread.Join();
            }

            System.Diagnostics.Process.Start(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\compLexity Demo Player.exe");
        }

        private void AppendToLog(String s)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Procedure(() =>
            {
                uiLogTextBox.Text += s;
                uiLogTextBox.ScrollToEnd();
            }));
        }

        private void PointOfNoReturn()
        {
            hasPassedThePointOfNoReturn = true;

            Dispatcher.Invoke(DispatcherPriority.Normal, new Procedure(() =>
            {
                uiCancelOrOkButton.Visibility = Visibility.Collapsed;
            }));
        }

        private void UpdateComplete(Boolean success)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Procedure(() =>
            {
                if (success)
                {
                    uiLogTextBox.Text += "\nUpdate installed successfully, click \"OK\" to continue.";
                    uiCancelOrOkButton.Content = "OK";
                }
                else
                {
                    uiLogTextBox.Text += "\nUpdate error.";

                    if (hasPassedThePointOfNoReturn)
                    {
                        uiLogTextBox.Text += " Since the error occured while in the process of downloading an update, it's recommended that you re-install the program.";
                    }
                }

                uiLogTextBox.ScrollToEnd();
                uiCancelOrOkButton.Visibility = Visibility.Visible;
            }));

            hasPassedThePointOfNoReturn = false;
        }

        private void uiCancelOrOkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
