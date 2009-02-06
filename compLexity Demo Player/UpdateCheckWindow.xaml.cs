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
using System.Windows.Media.Animation;
using System.Threading;
using System.Windows.Threading;

namespace compLexity_Demo_Player
{
    /// <summary>
    /// The "checking for program updates" window.
    /// <remarks> 
    /// If an UpdateCheck object is passed to the constructor then the initial window state (the actual update check) is skipped. This is used by automatic updates, where this step should be invisible to the user.
    /// </remarks>
    /// </summary>
    public partial class UpdateCheckWindow : Window, IUpdateCheck
    {
        public enum State
        {
            Checking,
            Found,
            NotFound
        }

        private State state;
        private UpdateCheck updateCheck;
        private Procedure<String> downloadUpdate;

        public UpdateCheckWindow(Procedure<String> downloadUpdate, UpdateCheck updateCheck)
        {
            InitializeComponent();

            this.downloadUpdate = downloadUpdate;

            if (updateCheck == null)
            {
                SetState(State.Checking);
                this.updateCheck = new UpdateCheck((IUpdateCheck)this);
            }
            else
            {
                this.updateCheck = updateCheck;
                SetState(State.Found);
            }
        }

        public UpdateCheckWindow(Procedure<String> downloadUpdate)
            : this(downloadUpdate, null)
        {
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            updateCheck.Cancel();
        }

        private void uiCancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void uiCloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void uiYesButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            downloadUpdate(updateCheck.AvailableVersion);
        }

        private void uiNoButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SetState(State state)
        {
            this.state = state;

            switch (state)
            {
                case State.Checking:
                    uiCancelButton.Visibility = Visibility.Visible;
                    uiCloseButton.Visibility = Visibility.Collapsed;
                    uiYesButton.Visibility = Visibility.Collapsed;
                    uiNoButton.Visibility = Visibility.Collapsed;
                    uiStatusTextBlock.Text = "Please wait...";
                    break;

                case State.Found:
                    uiCancelButton.Visibility = Visibility.Collapsed;
                    uiCloseButton.Visibility = Visibility.Collapsed;
                    uiYesButton.Visibility = Visibility.Visible;
                    uiNoButton.Visibility = Visibility.Visible;
                    uiStatusTextBlock.Text = String.Format("An update is available. Current version: {0}, available version: {1}. Do you want to download and install it now?", Config.Settings.ProgramVersion, updateCheck.AvailableVersion);
                    break;

                case State.NotFound:
                    uiCancelButton.Visibility = Visibility.Collapsed;
                    uiCloseButton.Visibility = Visibility.Visible;
                    uiYesButton.Visibility = Visibility.Collapsed;
                    uiNoButton.Visibility = Visibility.Collapsed;
                    uiStatusTextBlock.Text = "Update not found.";

                    if (updateCheck.ErrorMessage == null)
                    {
                        uiStatusTextBlock.Text += " Program is up to date.";
                    }
                    else
                    {
                        uiStatusTextBlock.Text += " " + updateCheck.ErrorMessage;
                    }
                    break;
            }

            // center window
            if (Owner != null)
            {
                UpdateLayout();
                Left = Owner.Left + Owner.ActualWidth / 2.0 - ActualWidth / 2.0;
                Top = Owner.Top + Owner.ActualHeight / 2.0 - ActualHeight / 2.0;
            }
        }

        public void UpdateCheckComplete()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Procedure(() =>
            {
                if (updateCheck.Available)
                {
                    SetState(State.Found);
                }
                else
                {
                    SetState(State.NotFound);
                }
            }));
        }
    }
}
