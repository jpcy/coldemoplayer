using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Reflection;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Threading;
using compLexity_Demo_Player;

namespace Server_Browser
{
    public interface IMainWindow
    {
        void SetGotfragStatus(String status);
        void AddGotfragServer(GotfragServer server);
        void SetServerProperty(Server server, String propertyName, Object propertyValue);
        void AddPlayerToServer(Server server, Player player);
    }

    public partial class MainWindow : IMainWindow, ILauncher
    {
        /// <summary>
        /// Called by the Gotfrag update thread to set the current status, or hide the status text block (allowing the listview behind it to be displayed).
        /// </summary>
        /// <param name="status"></param>
        public void SetGotfragStatus(String status)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Procedure(() =>
            {
                if (status == null)
                {
                    uiGotfragListView.Visibility = Visibility.Visible;
                    uiGotfragStatusLabel.Visibility = Visibility.Hidden;
                }
                else
                {
                    uiGotfragListView.Visibility = Visibility.Hidden;
                    uiGotfragStatusLabel.Visibility = Visibility.Visible;
                }

                uiGotfragStatusLabel.Content = status;
            }));
        }

        public void AddGotfragServer(GotfragServer server)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Procedure(() =>
            {
                gotfragScoreboard.Servers.Add(server);
                server.Refresh();
            }));
        }

        public void SetServerProperty(Server server, String propertyName, Object propertyValue)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Procedure(() =>
            {
                PropertyInfo piMatch = Common.FirstOrDefault<PropertyInfo>(typeof(Server).GetProperties(), pi => pi.Name == propertyName);

                if (piMatch != null)
                {
                    piMatch.SetValue(server, propertyValue, null);
                }
                else
                {
                    Debug.Fail(String.Format("Invalid server property \"{0}\"", propertyName));
                }
            }));
        }

        public void AddPlayerToServer(Server server, Player player)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Procedure(() =>
            {
                if (server.Players == null)
                {
                    server.Players = new ObservableCollection<Player>();
                }

                server.Players.Add(player);
            }));
        }

        /// <summary>
        /// Called by the launcher thread when the process is found.
        /// </summary>
        public void LaunchedProcessFound()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Procedure(() =>
            {
                TrayContextMenuEnable(false);
                SystemTrayIcon.Text = "Waiting for game process to exit...";
            }));
        }

        /// <summary>
        /// Called by either the launcher thread when the process is closed, or by the context menu attached to the system tray icon when searching for the process is cancelled.
        /// </summary>
        public void LaunchedProcessClosed()
        {
            Procedure action = () =>
            {
                try
                {
                    FileOperationList.Execute();
                }
                catch (Exception)
                {
                    // suppress cleanup errors
                }

                // If the system tray still has a context menu, then connecting must have been cancelled.
                // Don't apply the "close when finished" functionality in this case.
                if (!SystemTrayIcon.HasContextMenuStrip && Config.Settings.ServerBrowserCloseWhenFinished)
                {
                    SystemTrayIcon.Hide();
                    closeProgram = true;
                    Close();
                }
                else
                {
                    RestoreFromTray();
                    canOpenServer = true;
                }
            };

            if (Thread.CurrentThread != Dispatcher.Thread)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, action);
            }
            else
            {
                action();
            }
        }
    }
}
