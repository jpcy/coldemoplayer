using System;
using System.Collections.ObjectModel;
using CDP.Core;
using Microsoft.Win32;

namespace CDP.HalfLife
{
    public class SettingsViewModel : Core.ViewModelBase
    {
        public bool Playdemo
        {
            get 
            {
                return ((Plugin.PlaybackMethods)settings["HlPlaybackMethod"] == Plugin.PlaybackMethods.Playdemo);
            }
            set
            {
                settings["HlPlaybackMethod"] = Plugin.PlaybackMethods.Playdemo;
            }
        }

        public bool Viewdemo
        {
            get
            {
                return ((Plugin.PlaybackMethods)settings["HlPlaybackMethod"] == Plugin.PlaybackMethods.Viewdemo);
            }
            set
            {
                settings["HlPlaybackMethod"] = Plugin.PlaybackMethods.Viewdemo;
            }
        }

        public bool StartListenServer
        {
            get { return (bool)settings["HlStartListenServer"]; }
            set { settings["HlStartListenServer"] = value; }
        }

        public bool RemoveShowscores
        {
            get { return (bool)settings["HlRemoveShowscores"]; }
            set { settings["HlRemoveShowscores"] = value; }
        }

        public bool RemoveHltvAds
        {
            get { return (bool)settings["HlRemoveHltvAds"]; }
            set { settings["HlRemoveHltvAds"] = value; }
        }

        public string SteamExeFullPath
        {
            get { return (string)settings["SteamExeFullPath"]; }
            set { settings["SteamExeFullPath"] = value; }
        }

        public string SteamAccountName
        {
            get { return (string)settings["SteamAccountName"]; }
            set { settings["SteamAccountName"] = value; }
        }

        public DelegateCommand BrowseForSteamExeCommand { get; private set; }
        public ObservableCollection<string> SteamAccountNames { get; private set; }

        protected readonly ISettings settings = ObjectCreator.Get<ISettings>();
        protected readonly IFileSystem fileSystem = ObjectCreator.Get<IFileSystem>();

        public SettingsViewModel()
        {
            BrowseForSteamExeCommand = new DelegateCommand(BrowseForSteamExeCommandExecute);
            SteamAccountNames = new ObservableCollection<string>();
            UpdateSteamAccountNames();
        }

        public void BrowseForSteamExeCommandExecute()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = Strings.BrowseForSteamExeDialogTitle,
                Filter = "Steam.exe|Steam.exe",
                RestoreDirectory = true
            };

            if (fileSystem.FileExists(SteamExeFullPath))
            {
                dialog.InitialDirectory = fileSystem.GetDirectoryName(SteamExeFullPath);
            }

            if (dialog.ShowDialog() == true)
            {
                SteamExeFullPath = dialog.FileName;
                OnPropertyChanged("SteamExeFullPath");
                UpdateSteamAccountNames();
            }
        }

        public void UpdateSteamAccountNames()
        {
            SteamAccountNames.Clear();

            if (string.IsNullOrEmpty(SteamExeFullPath) || !fileSystem.FileExists(SteamExeFullPath))
            {
                SteamAccountName = null;
                return;
            }

            string steamAppsPath = fileSystem.PathCombine(fileSystem.GetDirectoryName(SteamExeFullPath), "SteamApps");

            if (fileSystem.DirectoryExists(steamAppsPath))
            {
                foreach (string folder in fileSystem.GetFolderNames(steamAppsPath))
                {
                    SteamAccountNames.Add(folder);
                }
            }

            OnPropertyChanged("SteamAccountNames");
        }
    }
}
