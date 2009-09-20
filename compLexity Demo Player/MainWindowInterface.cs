using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Threading;

namespace compLexity_Demo_Player
{
    public interface IMainWindow
    {
        Boolean OpenDemo(String fileName);
        Boolean OpenServer(String hlswAddress);
        void Error(String errorMessage, Exception ex);
    }

    public partial class MainWindow : IMainWindow, ILauncher, IUpdateCheck
    {
        // called from this class, as well as from Main via IPC (which is why there's a canOpenDemo check)
        public Boolean OpenDemo(String fileName)
        {
            Function<String, Boolean> func = delegate
            {
                if (!canOpenDemo)
                {
                    return false;
                }

                // Restore the window if the program is minimized to the tray.
                RestoreFromTray();

                // select demo, but don't trigger the event handler
                String path = System.IO.Path.GetDirectoryName(fileName);
                uiExplorerTreeView.SelectedItemChanged -= uiExplorerTreeView_SelectedItemChanged;
                uiExplorerTreeView.CurrentFolderPath = path;
                uiDemoListView.SetCurrentPath(uiExplorerTreeView.CurrentFolderPath, System.IO.Path.GetFileNameWithoutExtension(fileName));
                uiExplorerTreeView.SelectedItemChanged += uiExplorerTreeView_SelectedItemChanged;

                return true;
            };

            if (Thread.CurrentThread != Dispatcher.Thread)
            {
                return (Boolean)Dispatcher.Invoke(DispatcherPriority.Normal, func, fileName);
            }
            else
            {
                return func(fileName);
            }
        }

        public Boolean OpenServer(String hlswAddress)
        {
            Function<String, Boolean> func = delegate
            {
                if (!canOpenDemo && !serverWindowOpen)
                {
                    return false;
                }

                // Restore the window if the program is minimized to the tray.
                if (!serverWindowOpen)
                {
                    RestoreFromTray();
                }

                ShowServerBrowserWindow();
                String hlswProtocol = "hlsw://";
                serverBrowserWindow.OpenServer(hlswAddress.Remove(0, hlswProtocol.Length).TrimEnd('/'));
                return true;
            };

            if (Thread.CurrentThread != Dispatcher.Thread)
            {
                return (Boolean)Dispatcher.Invoke(DispatcherPriority.Normal, func, hlswAddress);
            }
            else
            {
                return func(hlswAddress);
            }
        }

        /// <summary>
        /// Callback for automatic updates. Called by the UpdateCheck thread upon completion.
        /// </summary>
        public void UpdateCheckComplete()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Procedure(() =>
            {
                // see if an update is available
                if (!updateCheck.Available)
                {
                    return;
                }

                // ignore if subwindows are open
                if (!canOpenDemo)
                {
                    return;
                }

                UpdateCheckWindow window = new UpdateCheckWindow(ShowUpdateWindow, updateCheck);
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
            }));
        }

        public void LaunchedProcessFound()
        {
            Procedure action = () =>
            {
                TrayContextMenuEnable(false);
                SystemTrayIcon.Text = "Waiting for " + (UseHlae(uiDemoListView.GetSelectedDemo()) ? "HLAE" : "game") + " process to exit...";
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

                Demo demo = uiDemoListView.GetSelectedDemo();
                if (demo != null && UseHlae(demo))
                {
                    Debug.Assert(launcher as SteamLauncher != null);
                }

                // If the system tray still has a context menu, then connecting must have been cancelled.
                // Don't apply the "close when finished" functionality in this case.
                if (!SystemTrayIcon.HasContextMenuStrip && Config.Settings.PlaybackCloseWhenFinished)
                {
                    SystemTrayIcon.Hide();
                    Close();
                }
                else
                {
                    RestoreFromTray();
                    canOpenDemo = true;
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

        public void Error(String errorMessage, Exception ex)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Procedure(() =>
            {
                Common.Message(this, errorMessage, ex, MessageWindow.Flags.Error);
            }));
        }
    }
}
