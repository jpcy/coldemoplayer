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
using System.Data; // DataTable
using System.Diagnostics; // Process
using System.Windows.Threading;
using System.Collections.ObjectModel; // ObservableCollection

namespace compLexity_Demo_Player
{
    public partial class MainWindow : Window
    {
        private class PlayerListViewData : NotifyPropertyChangedItem
        {
            public PlayerListViewData(HalfLifeDemo.Player player)
            {
                Name = player.InfoKeys["name"];
                UpdateRate = player.InfoKeys["cl_updaterate"];
                Rate = player.InfoKeys["rate"];

                String sid = player.InfoKeys["*sid"];

                // "*sid" only exists in protocol 48 Half-Life demos. And even then it's common for people to "convert" protocol 47 demos to 48, so it's best to make sure the infokey value exists.
                if (player.InfoKeys["*hltv"] != null)
                {
                    SteamId = "HLTV";
                }
                else if (sid != null)
                {
                    SteamId = Common.CalculateSteamId(sid);

                    if (SteamId == null)
                    {
                        SteamId = "-";
                    }
                }
                else
                {
                    SteamId = "-";
                }
            }

            public PlayerListViewData(SourceDemo.Player player)
            {
                Name = player.Name;
                SteamId = player.SteamId;
            }

            public String Name { get; private set; }
            
            // goldsrc
            public String UpdateRate { get; private set; }
            public String Rate { get; private set; }

            // source
            public String SteamId { get; private set; }
        }

        private class GameProcessInformation
        {
            public String ExeFullPath { get; set; }
            public Demo Demo { get; set; }
        }

        private Boolean hasActivated = false;
        private Boolean canOpenDemo = true;
        private Boolean serverWindowOpen = false;
        private Launcher launcher;
        private UpdateCheck updateCheck = null;
        private WindowState lastWindowState; // used for "minimize to tray" restore
        private ObservableCollection<PlayerListViewData> playerCollection;
        private Server_Browser.MainWindow serverBrowserWindow;

        public MainWindow()
        {
            InitializeComponent();
            Program.MainWindowInterface = this;
        }

        private ExtendedGridViewColumn CreateGridViewColumn(String header, String displayMemberBinding)
        {
            ExtendedGridViewColumn gvc = new ExtendedGridViewColumn();
            gvc.Header = header;
            gvc.DisplayMemberBinding = new Binding(displayMemberBinding);
            gvc.SortPropertyName = displayMemberBinding;
            return gvc;
        }

        private void SetPlayerListGridView(Demo demo)
        {
            GridView gv = new GridView();

            ExtendedGridViewColumn nameColumn = CreateGridViewColumn("Name", "Name");
            nameColumn.IsDefaultSortColumn = true;
            nameColumn.IsLowPriority = true;
            gv.Columns.Add(nameColumn);

            if (demo.Engine != Demo.Engines.Source)
            {
                gv.Columns.Add(CreateGridViewColumn("cl__updaterate", "UpdateRate"));
                gv.Columns.Add(CreateGridViewColumn("rate", "Rate"));                
            }

            if (demo.Engine == Demo.Engines.Source || demo.NetworkProtocol >= 48)
            {
                gv.Columns.Add(CreateGridViewColumn("Steam ID", "SteamId"));
            }

            uiPlayersListView.View = gv;
        }

        /// <summary>
        /// Displays the window that handles downloading program updates.
        /// </summary>
        private void ShowUpdateWindow(String updateVersion)
        {
            UpdateWindow window = new UpdateWindow(updateVersion);
            window.Owner = this;

            try
            {
                canOpenDemo = false;
                window.ShowDialog();
            }
            finally
            {
                canOpenDemo = true;
            }
        }

        private void UpdateOptionsCheckboxes()
        {
            Demo demo = uiDemoListView.GetSelectedDemo();

            // copy checkbox state into config settings
            Config.Settings.PlaybackRemoveShowscores = (Boolean)uiOptionsRemoveShowscoresCheckBox.IsChecked;
            Config.Settings.PlaybackRemoveFtb = (Boolean)uiOptionsRemoveFtbCheckBox.IsChecked;
            Config.Settings.PlaybackRemoveHltvAds = (Boolean)uiOptionsRemoveHltvAds.IsChecked;
            Config.Settings.PlaybackRemoveHltvSlowMotion = (Boolean)uiOptionsRemoveHltvSlowMotion.IsChecked;
            Config.Settings.PlaybackStartListenServer = (Boolean)uiOptionsStartListenServerCheckBox.IsChecked;
            Config.Settings.PlaybackConvertNetworkProtocol = (Boolean)uiOptionsConvertNetworkProtocolCheckBox.IsChecked;
            Config.Settings.PlaybackCloseWhenFinished = (Boolean)uiOptionsCloseWhenFinishedCheckBox.IsChecked;
            Config.Settings.PlaybackUseHlae = (Boolean)uiOptionsUseHlaeCheckBox.IsChecked;

            // disable all checkboxes
            uiOptionsRemoveShowscoresCheckBox.IsEnabled = false;
            uiOptionsRemoveFtbCheckBox.IsEnabled = false;
            uiOptionsRemoveHltvAds.IsEnabled = false;
            uiOptionsRemoveHltvSlowMotion.IsEnabled = false;
            uiOptionsStartListenServerCheckBox.IsEnabled = false;
            uiOptionsConvertNetworkProtocolCheckBox.IsEnabled = false;
            uiOptionsCloseWhenFinishedCheckBox.IsEnabled = false;
            uiOptionsUseHlaeCheckBox.IsEnabled = false;

            if (demo == null)
            {
                return;
            }

            // work out which checkboxes should be enabled
            uiOptionsCloseWhenFinishedCheckBox.IsEnabled = true;

            if (demo.Engine != Demo.Engines.Source)
            {
                if (demo.Perspective == Demo.Perspectives.Pov)
                {
                    uiOptionsRemoveShowscoresCheckBox.IsEnabled = true;
                }

                if (GameManager.CanConvertNetworkProtocol(demo))
                {
                    uiOptionsConvertNetworkProtocolCheckBox.IsEnabled = true;
                }
            }

            if ((demo.Engine == Demo.Engines.HalfLifeSteam && demo.GameFolderName != "tfc") || (demo.Engine == Demo.Engines.HalfLife && uiOptionsConvertNetworkProtocolCheckBox.IsEnabled && (Boolean)uiOptionsConvertNetworkProtocolCheckBox.IsChecked))
            {
                uiOptionsStartListenServerCheckBox.IsEnabled = true;
            }

            if (GameManager.CanRemoveFadeToBlack(demo))
            {
                uiOptionsRemoveFtbCheckBox.IsEnabled = true;
            }

            if (GameManager.CanRemoveHltvAds(demo))
            {
                uiOptionsRemoveHltvAds.IsEnabled = true;
            }

            if (GameManager.CanRemoveHltvSlowMotion(demo))
            {
                uiOptionsRemoveHltvSlowMotion.IsEnabled = true;
            }

            if (CanUseHlae(demo))
            {
                uiOptionsUseHlaeCheckBox.IsEnabled = true;
            }
        }

        private Boolean CanUseHlae(Demo demo)
        {
            if (demo.Engine == Demo.Engines.HalfLifeSteam || (demo.Engine == Demo.Engines.HalfLife && ((HalfLifeDemo)demo).ConvertNetworkProtocol()))
            {
                return true;
            }

            return false;
        }

        private Boolean UseHlae(Demo demo)
        {
            if (Config.Settings.PlaybackUseHlae && CanUseHlae(demo))
            {
                return true;
            }

            return false;
        }

        private void ShowPreferencesWindow(Boolean firstRun)
        {
            PreferencesWindow window = new PreferencesWindow(firstRun);
            window.Owner = this;

            try
            {
                canOpenDemo = false;
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                window.Close();
                Common.Message(this, "Preferences window error.", ex, MessageWindow.Flags.Error);
            }
            finally
            {
                canOpenDemo = true;
            }
        }

        private void ShowServerBrowserWindow()
        {
            if (serverBrowserWindow == null || !serverBrowserWindow.IsActive)
            {
                serverBrowserWindow = new Server_Browser.MainWindow(new Procedure(delegate
                {
                    // Has the server browser window requested that the program be closed?
                    if (serverBrowserWindow.CloseProgram == true)
                    {
                        Close();
                        return;
                    }

                    Show();
                    canOpenDemo = true;
                    serverWindowOpen = false;
                }));
            }

            try
            {
                canOpenDemo = false;
                serverWindowOpen = true;
                Hide();
                serverBrowserWindow.Show();
            }
            catch (Exception ex)
            {
                serverBrowserWindow.Close();
                Show();
                canOpenDemo = true;
                serverWindowOpen = false;
                Common.Message(this, "Server Browser window error.", ex, MessageWindow.Flags.Error);
            }
        }

        #region Tray
        private void MinimiseToTray()
        {
            Hide();
            SystemTrayIcon.Show(new EventHandler(notifyIcon_Click));
        }

        private void RestoreFromTray()
        {
            if (SystemTrayIcon.IsVisible)
            {
                Show();
                SystemTrayIcon.Hide();
            }
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            if (canOpenDemo)
            {
                RestoreFromTray();
                WindowState = lastWindowState;
            }
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
                menuItem.Click += new EventHandler(CancelDemoPlaybackToolStripMenuItem_Click);
                contextMenuStrip.Items.Add(menuItem);
                SystemTrayIcon.SetContextMenuStrip(contextMenuStrip);
            }
        }
        #endregion

        #region Event handlers
        private void Window_Initialized(object sender, EventArgs e)
        {
            // set the window state
            WindowState = Config.Settings.WindowState;
            Width = Config.Settings.WindowWidth;
            Height = Config.Settings.WindowHeight;
            uiExplorerColumnDefinition.Width = new GridLength(Config.Settings.ExplorerPaneWidth);
            uiDemoListRowDefinition.Height = new GridLength(Config.Settings.DemoListPaneHeight);
            
            // playback type
            uiPlaydemoRadioButton.IsChecked = (Config.Settings.PlaybackType == ProgramSettings.Playback.Playdemo ? true : false);
            uiViewdemoRadioButton.IsChecked = !uiPlaydemoRadioButton.IsChecked;

            // player list
            playerCollection = new ObservableCollection<PlayerListViewData>();
            uiPlayersListView.ItemsSource = playerCollection;

            // options
            uiOptionsRemoveShowscoresCheckBox.IsChecked = Config.Settings.PlaybackRemoveShowscores;
            uiOptionsRemoveFtbCheckBox.IsChecked = Config.Settings.PlaybackRemoveFtb;
            uiOptionsRemoveHltvAds.IsChecked = Config.Settings.PlaybackRemoveHltvAds;
            uiOptionsRemoveHltvSlowMotion.IsChecked = Config.Settings.PlaybackRemoveHltvSlowMotion;
            uiOptionsStartListenServerCheckBox.IsChecked = Config.Settings.PlaybackStartListenServer;
            uiOptionsConvertNetworkProtocolCheckBox.IsChecked = Config.Settings.PlaybackConvertNetworkProtocol;
            uiOptionsCloseWhenFinishedCheckBox.IsChecked = Config.Settings.PlaybackCloseWhenFinished;
            uiOptionsUseHlaeCheckBox.IsChecked = Config.Settings.PlaybackUseHlae;

            uiOptionsRemoveShowscoresCheckBox.Checked += new RoutedEventHandler(uiOptions_Changed);
            uiOptionsRemoveShowscoresCheckBox.Unchecked += new RoutedEventHandler(uiOptions_Changed);
            uiOptionsRemoveFtbCheckBox.Checked += new RoutedEventHandler(uiOptions_Changed);
            uiOptionsRemoveFtbCheckBox.Unchecked += new RoutedEventHandler(uiOptions_Changed);
            uiOptionsRemoveHltvAds.Checked += new RoutedEventHandler(uiOptions_Changed);
            uiOptionsRemoveHltvAds.Unchecked += new RoutedEventHandler(uiOptions_Changed);
            uiOptionsRemoveHltvSlowMotion.Checked += new RoutedEventHandler(uiOptions_Changed);
            uiOptionsRemoveHltvSlowMotion.Unchecked += new RoutedEventHandler(uiOptions_Changed);
            uiOptionsStartListenServerCheckBox.Checked += new RoutedEventHandler(uiOptions_Changed);
            uiOptionsStartListenServerCheckBox.Unchecked += new RoutedEventHandler(uiOptions_Changed);
            uiOptionsConvertNetworkProtocolCheckBox.Checked += new RoutedEventHandler(uiOptions_Changed);
            uiOptionsConvertNetworkProtocolCheckBox.Unchecked += new RoutedEventHandler(uiOptions_Changed);
            uiOptionsCloseWhenFinishedCheckBox.Checked += new RoutedEventHandler(uiOptions_Changed);
            uiOptionsCloseWhenFinishedCheckBox.Unchecked += new RoutedEventHandler(uiOptions_Changed);
            uiOptionsUseHlaeCheckBox.Checked += new RoutedEventHandler(uiOptions_Changed);
            uiOptionsUseHlaeCheckBox.Unchecked += new RoutedEventHandler(uiOptions_Changed);

            UpdateOptionsCheckboxes();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (hasActivated)
            {
                return;
            }

            hasActivated = true;

            // if running for the first time, prompt the user to setup their Steam account details
            if (Program.FirstRun)
            {
                Common.Message(this, Config.ProgramName + " is running for the first time. Please ensure the detected Steam path and the selected Steam account folder are correct.");
                ShowPreferencesWindow(true);
            }

            // start auto update thread
            if (Config.Settings.AutoUpdate)
            {
                updateCheck = new UpdateCheck((IUpdateCheck)this);
            }

            // check for a file/server address in the command line arguments (file/protocol association)
            String[] commandLineArgs = Environment.GetCommandLineArgs();

            if (commandLineArgs.Length > 1)
            {
                if (commandLineArgs[1].StartsWith("hlsw://"))
                {
                    if (OpenServer(commandLineArgs[1]))
                    {
                        return; // stop now, don't go to last folder/file
                    }
                }
                else
                {
                    String fullPath = System.IO.Path.GetFullPath(commandLineArgs[1]);

                    if (System.IO.File.Exists(fullPath))
                    {
                        if (OpenDemo(fullPath))
                        {
                            return; // stop now, don't go to last folder/file
                        }
                    }
                }
            }

            // open last folder/file
            if (System.IO.Directory.Exists(Config.Settings.LastPath))
            {
                String lastFileName = Config.Settings.LastPath + "\\" + Config.Settings.LastFileName;

                if (System.IO.File.Exists(lastFileName))
                {
                    OpenDemo(lastFileName);
                }
                else
                {
                    uiExplorerTreeView.CurrentFolderPath = Config.Settings.LastPath;
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // store the last path and filename
            Config.Settings.LastPath = uiExplorerTreeView.CurrentFolderPath;

            Demo selectedDemo = uiDemoListView.GetSelectedDemo();
            if (selectedDemo != null)
            {
                Config.Settings.LastFileName = selectedDemo.Name + ".dem";
            }
            else
            {
                Config.Settings.LastFileName = "";
            }

            // store the window state
            Config.Settings.WindowState = (WindowState == WindowState.Minimized ? WindowState.Normal : WindowState);
            Config.Settings.WindowWidth = Width;
            Config.Settings.WindowHeight = Height;
            Config.Settings.ExplorerPaneWidth = uiExplorerColumnDefinition.Width.Value;
            Config.Settings.DemoListPaneHeight = uiDemoListRowDefinition.Height.Value;

            // write the config file
            Config.Write();
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

                if (s.Length == 1 && s[0].EndsWith(".dem", StringComparison.CurrentCultureIgnoreCase))
                {
                    e.Effects = DragDropEffects.Link;
                }
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            if (s.Length != 1)
            {
                return;
            }

            OpenDemo(Common.SanitisePath(s[0]));
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

        private void uiOpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // open file dialog
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();

            dialog.Title = "Open Demo";
            dialog.Filter = "Demo files (*.dem)|*.dem";
            dialog.RestoreDirectory = true;

            canOpenDemo = false;

            if (dialog.ShowDialog(this) == false)
            {
                canOpenDemo = true;
                return;
            }

            // open demo
            canOpenDemo = true;
            OpenDemo(Common.SanitisePath(dialog.FileName));
        }

        private void uiExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void uiPreferencesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowPreferencesWindow(false);
        }

        private void uiServerBrowserMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowServerBrowserWindow();
        }

        private void uiMapPoolMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MapPoolWindow window = new MapPoolWindow();
            window.Owner = this;

            try
            {
                canOpenDemo = false;
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                window.Close();
                Common.Message(this, "Map pool window error.", ex, MessageWindow.Flags.Error);
            }
            finally
            {
                canOpenDemo = true;
            }
        }

        private void uiRestoreWindowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
            Width = 800.0;
            Height = 600.0;
            uiExplorerColumnDefinition.Width = new GridLength(320);
            uiDemoListRowDefinition.Height = new GridLength(150);
        }

        private void uiCheckForUpdatesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // cancel the automatic update thread if it's already running
            if (updateCheck != null)
            {
                updateCheck.Cancel();
            }

            UpdateCheckWindow window = new UpdateCheckWindow(ShowUpdateWindow) { Owner = this };

            try
            {
                canOpenDemo = false;
                window.ShowDialog();
            }
            finally
            {
                canOpenDemo = true;
            }
        }

        private void uiReadmeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(Config.ProgramPath + "\\readme.txt");
            }
            catch (Exception)
            {
                // don't care about errors
            }
        }

        private void uiAboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow window = new AboutWindow() { Owner = this };

            try
            {
                canOpenDemo = false;
                window.ShowDialog();
            }
            finally
            {
                canOpenDemo = true;
            }
        }

        private void uiBannerGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start(Config.ComplexityUrl);
        }

        private void uiPlayButton_Click(object sender, RoutedEventArgs e)
        {
            Demo demo = uiDemoListView.GetSelectedDemo();

            if (demo == null)
            {
                // shouldn't happen since the button should be disabled, but anyway...
                return;
            }

            // verify paths etc.
            launcher = LauncherFactory.CreateLauncher(demo);
            launcher.Owner = this;
            launcher.UseHlae = UseHlae(demo);
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

            // make sure source and destination filenames don't match (dickhead insurance)
            if (String.Equals(demo.FileFullPath, launcher.GameFullPath + "\\" + Config.LaunchDemoFileName, StringComparison.CurrentCultureIgnoreCase))
            {
                Common.Message(this, "Source and destination filenames are the same. Rename or move the source file and try again.");
                return;
            }

            // add the demo file path to the file operation manager
            FileOperationList.Add(new FileDeleteOperation(launcher.GameFullPath + "\\" + Config.LaunchDemoFileName));

            // stop the user from being able to open a new demo now
            canOpenDemo = false;

            // write the demo
            ProgressWindow writeDemoProgressWindow = new ProgressWindow() { Owner = this };
            writeDemoProgressWindow.Thread = demo.Write((IProgressWindow)writeDemoProgressWindow);
            writeDemoProgressWindow.ThreadParameter = launcher.GameFullPath + "\\" + Config.LaunchDemoFileName;
            if (writeDemoProgressWindow.ShowDialog() == false)
            {
                // user aborted writing
                FileOperationList.Execute();
                canOpenDemo = true;
                return;
            }

            // go to tray
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
                canOpenDemo = true;
                Common.Message(this, "Error launching game.", ex, MessageWindow.Flags.Error);
                return;
            }

            SystemTrayIcon.Text = "Looking for " + (launcher.UseHlae ? "HLAE" : "game") + " process...";

            // enable the tray context menu
            TrayContextMenuEnable(true);
        }

        private void uiAnalyseButton_Click(object sender, RoutedEventArgs e)
        {
            Demo demo = uiDemoListView.GetSelectedDemo();
            Debug.Assert(demo != null);

            AnalysisWindow window = AnalysisWindowFactory.CreateAnalysisWindow(demo);
            window.Owner = this;
            canOpenDemo = false;

            try
            {
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                window.Close();
                Common.Message(this, "Analysis window error.", ex, MessageWindow.Flags.Error);
            }
            finally
            {
                canOpenDemo = true;
            }
        }

        private void uiExplorerTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            uiDemoListView.SetCurrentPath(uiExplorerTreeView.CurrentFolderPath);
        }

        private void uiDemoListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: remember last preview/overview images loaded and don't reload them
            Demo demo = uiDemoListView.GetSelectedDemo();

            // clear controls
            uiMapTextBlock.Text = "";
            uiMapPreviewImage.Source = null;
            uiMapOverviewImage.Source = null;

            uiDetailsStatusTextBox.Text = "-";
            uiDetailsSteamTextBox.Text = "-";
            uiDetailsServerNameTextBox.Text = "-";
            uiDetailsServerSlotsTextBox.Text = "-";
            uiDetailsRecordedByTextBox.Text = "-";
            uiDetailsEngineVersionTextBox.Text = "-";
            uiDetailsGameFolderTextBox.Text = "-";
            uiDetailsBuildNumberTextBox.Text = "-";
            uiDetailsProtocolTextBox.Text = "-";
            uiDetailsMapChecksumTextBox.Text = "-";

            playerCollection.Clear();

            // options tab
            UpdateOptionsCheckboxes();

            uiPlayButton.IsEnabled = false;
            uiAnalyseButton.IsEnabled = false;

            if (demo == null)
            {
                return;
            }

            // map tab
            uiMapTextBlock.Text = demo.MapName;
            String engineFolder = (demo.Engine == Demo.Engines.Source ? "source" : "goldsrc");
            String imagePartialPath = "\\" + engineFolder + "\\" + demo.GameFolderName + "\\" + demo.MapName + ".jpg";
            try
            {
                uiMapPreviewImage.Source = new BitmapImage(new Uri(Config.ProgramPath + "\\previews" + imagePartialPath));
            }
            catch 
            {
            }

            try
            {
                uiMapOverviewImage.Source = new BitmapImage(new Uri(Config.ProgramPath + "\\overviews" + imagePartialPath));
            }
            catch
            {
            }

            // details tab
            if (demo.Status == Demo.StatusEnum.Ok)
            {
                uiDetailsStatusTextBox.Foreground = Brushes.Green;
                uiDetailsStatusTextBox.Text = "OK";
            }
            else
            {
                uiDetailsStatusTextBox.Foreground = Brushes.Red;
                uiDetailsStatusTextBox.Text = "Corrupt directory entries";
            }

            uiDetailsSteamTextBox.Text = (demo.Engine == Demo.Engines.HalfLife ? "No" : "Yes");
            uiDetailsServerNameTextBox.Text = demo.ServerName;
            uiDetailsServerSlotsTextBox.Text = String.Format("{0}", demo.MaxClients);

            if (demo.Perspective == Demo.Perspectives.Pov)
            {
                uiDetailsRecordedByTextBox.Text = demo.RecorderName;
            }
            else
            {
                uiDetailsRecordedByTextBox.Text = demo.PerspectiveString;
            }

            uiDetailsEngineVersionTextBox.Text = demo.EngineName;
            uiDetailsGameFolderTextBox.Text = demo.GameFolderName;

            if (demo.Perspective == Demo.Perspectives.Pov)
            {
                uiDetailsBuildNumberTextBox.Text = String.Format("{0}", demo.BuildNumber);
            }

            uiDetailsProtocolTextBox.Text = String.Format("{0}/{1}", demo.DemoProtocol, demo.NetworkProtocol);

            if (demo.MapChecksum != 0)
            {
                uiDetailsMapChecksumTextBox.Text = String.Format("{0}", demo.MapChecksum);
            }

            // players tab
            SetPlayerListGridView(demo);
            uiPlayersListView.Initialise();

            if (demo.Engine == Demo.Engines.Source)
            {
                foreach (SourceDemo.Player player in ((SourceDemo)demo).PlayerList)
                {
                    playerCollection.Add(new PlayerListViewData(player));
                }
            }
            else
            {
                foreach (HalfLifeDemo.Player player in ((HalfLifeDemo)demo).Players)
                {
                    playerCollection.Add(new PlayerListViewData(player));
                }
            }

            uiPlayersListView.Sort();

            // operations
            uiPlayButton.IsEnabled = true;
            uiAnalyseButton.IsEnabled = (demo == null ? false : GameManager.CanAnalyse(demo));
        }

        private void uiPlaybackType_Changed(object sender, RoutedEventArgs e)
        {
            Config.Settings.PlaybackType = (uiPlaydemoRadioButton.IsChecked == true ? ProgramSettings.Playback.Playdemo : ProgramSettings.Playback.Viewdemo);
        }

        private void uiOptions_Changed(object sender, RoutedEventArgs e)
        {
            UpdateOptionsCheckboxes();
        }

        private void CancelDemoPlaybackToolStripMenuItem_Click(object sender, EventArgs e)
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
}
