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
using System.Windows.Shapes;
using System.Threading;
using System.Diagnostics; // ProcessPriorityClass
using System.IO;
using Microsoft.Win32; // Registry
using compLexity_Demo_Player;

namespace Server_Browser
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Used by the main program window to determine if it should be closed (i.e. the program should close).
        /// Set to true when the game process being monitored closes, and if the user has "close when finished" selected.
        /// </summary>
        public Boolean CloseProgram
        {
            get
            {
                return closeProgram;
            }
        }

        private Procedure windowClosed;
        private Favourites favourites;
        private WindowState lastWindowState;
        private Launcher launcher;
        private Boolean canOpenServer = true;
        private Boolean closeProgram = false;

        public MainWindow(Procedure windowClosed)
        {
            this.windowClosed = windowClosed;
            InitializeComponent();

            // stop the retarded thing from triggering an event before the window has been created (in InitializeComponent())
            uiServerBrowserTabControl.SelectionChanged += new SelectionChangedEventHandler(uiServerBrowserTabControl_SelectionChanged);
        }

        public void OpenServer(String address)
        {
            RestoreFromTray();
            uiFavouritesListView.SelectedItem = favourites.Add(address);
        }

        private void ConnectToServer(Server server)
        {
            if (server == null)
            {
                return;
            }

            if (server.State != Server.StateEnum.Succeeded)
            {
                return;
            }

            launcher = LauncherFactory.CreateLauncher(server.SourceEngine);
            launcher.JoiningServer = true;
            launcher.ServerSourceEngine = server.SourceEngine;
            launcher.ServerAddress = server.Address;
            launcher.ServerGameFolderName = server.GameFolder;
            launcher.ServerMapName = server.Map;
            launcher.Interface = (ILauncher)this;

            try
            {
                launcher.Verify();
            }
            catch (Launcher.AbortLaunchException)
            {
                return;
            }
            catch (ApplicationException ex)
            {
                Common.Message(this, null, ex);
                return;
            }
            catch (Exception ex)
            {
                Common.Message(this, null, ex, MessageWindow.Flags.Error);
                return;
            }

            canOpenServer = false;
            MinimiseToTray();

            // launch the game
            try
            {
                launcher.Run();
            }
            catch (Exception ex)
            {
                RestoreFromTray();
                FileOperationList.Execute();
                canOpenServer = true;
                Common.Message(this, "Error launching game.", ex, MessageWindow.Flags.Error);
                return;
            }

            SystemTrayIcon.Text = "Looking for game process...";

            // enable the tray context menu
            TrayContextMenuEnable(true);
        }

        private void TrayContextMenuEnable(Boolean value)
        {
            if (!value)
            {
                SystemTrayIcon.SetContextMenuStrip(null); // disable context menu
            }
            else
            {
                System.Windows.Forms.ContextMenuStrip contextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
                System.Windows.Forms.ToolStripMenuItem menuItem = new System.Windows.Forms.ToolStripMenuItem();
                menuItem.Text = "Cancel";
                menuItem.Click += new EventHandler(CancelToolStripMenuItem_Click);
                contextMenuStrip.Items.Add(menuItem);
                SystemTrayIcon.SetContextMenuStrip(contextMenuStrip);
            }
        }

        private void MinimiseToTray()
        {
            Hide();
            SystemTrayIcon.Show(new EventHandler(notifyIcon_Click));
        }

        private void RestoreFromTray()
        {
            if (SystemTrayIcon.IsVisible)
            {
                SystemTrayIcon.Hide();
                Show();
                WindowState = lastWindowState;
            }
        }

        #region Event Handlers
        private void Window_Initialized(object sender, EventArgs e)
        {
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            favourites = new Favourites((IMainWindow)this);
            uiFavouritesListView.ItemsSource = favourites.Servers;

            if (Config.Settings.ServerBrowserFavourites != null)
            {
                foreach (String s in Config.Settings.ServerBrowserFavourites)
                {
                    favourites.Add(s);
                }
            }

            uiOptionsStartListenServerCheckBox.IsChecked = Config.Settings.ServerBrowserStartListenServer;
            uiOptionsCloseWhenFinishedCheckBox.IsChecked = Config.Settings.ServerBrowserCloseWhenFinished;
            WindowState = Config.Settings.ServerBrowserWindowState;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (!Config.Settings.MinimizeToTray)
            {
                return;
            }

            if (WindowState == WindowState.Minimized)
            {
                MinimiseToTray();
            }
            else
            {
                lastWindowState = WindowState;
            }
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            if (canOpenServer) // don't want to restore from the tray when we're connecting to a server
            {
                RestoreFromTray();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            favourites.AbortAllThreads();

            if (favourites.Servers.Count > 0)
            {
                Config.Settings.ServerBrowserFavourites = new String[favourites.Servers.Count];

                Int32 i = 0;
                foreach (Server server in favourites.Servers)
                {
                    Config.Settings.ServerBrowserFavourites[i++] = server.Address;
                }
            }
            else
            {
                Config.Settings.ServerBrowserFavourites = null;
            }

            Config.Settings.ServerBrowserWindowState = (WindowState == WindowState.Minimized ? WindowState.Normal : WindowState);
            windowClosed();
        }

        private void uiServerBrowserTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Object dataContext = null;

            if (uiServerBrowserTabControl.SelectedItem == uiFavouritesTabItem)
            {
                dataContext = uiFavouritesListView.SelectedItem;
                uiServerInfoAddressStackPanel.Visibility = Visibility.Collapsed;
                uiServerInfoGroupBox.Visibility = Visibility.Visible;
                Grid.SetRowSpan(uiServerBrowserTabControl, 1);
            }
            else
            {
                uiServerInfoGroupBox.Visibility = Visibility.Hidden;
                Grid.SetRowSpan(uiServerBrowserTabControl, 2);
            }

            uiServerInfoGroupBox.DataContext = dataContext;
        }

        private void uiFavouritesRefreshServerListButton_Click(object sender, RoutedEventArgs e)
        {
            favourites.RefreshAll();
        }

        private void uiFavouritesAddButton_Click(object sender, RoutedEventArgs e)
        {
            AddServerWindow window = new AddServerWindow();
            window.Owner = this;

            try
            {
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                window.Close();
                Common.Message(this, "Add server window error.", ex, MessageWindow.Flags.Error);
                return;
            }

            if (window.AddServer)
            {
                favourites.Add(window.ServerAddress);
            }
        }

        private void uiFavouritesRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (Common.Message(this, "Are you sure you want to remove the selected server?", null, MessageWindow.Flags.YesNo) == MessageWindow.Result.Yes)
            {
                favourites.RemoveServer((Server)uiFavouritesListView.SelectedItem);
            }
        }

        private void uiFavouritesRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Server server = (Server)uiFavouritesListView.SelectedItem;

            if (server != null)
            {
                server.Refresh();
            }
        }

        private void uiFavouritesConnectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectToServer((Server)uiFavouritesListView.SelectedItem);
        }

        private void uiOptionsStartListenServerCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Config.Settings.ServerBrowserStartListenServer = (Boolean)uiOptionsStartListenServerCheckBox.IsChecked;
        }

        private void uiOptionsCloseWhenFinishedCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Config.Settings.ServerBrowserCloseWhenFinished = (Boolean)uiOptionsCloseWhenFinishedCheckBox.IsChecked;
        }

        private void CancelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Make sure that the context menu is actually enabled. It's possible to right click to pop up the menu and just leave it open, then when the menu is disabled it doesn't disappear. The user can then erroneously activate the menu item.
            if (!SystemTrayIcon.HasContextMenuStrip)
            {
                return;
            }

            launcher.AbortProcessMonitor();
            LaunchedProcessClosed();
        }
        #endregion
    }

    // TODO: make subclass
    public class NotificationTime
    {
        public String Name { get; set; }
        public Int32 Minutes { get; set; }
        public TimeSpan TimeSpan
        {
            get
            {
                return new TimeSpan(0, Minutes, 0);
            }
        }
    }

    // TODO: make subclass
    public class ProcessPriority
    {
        public String Name { get; set; }
        public ProcessPriorityClass Value { get; set; }
    }
}
