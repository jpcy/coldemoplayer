using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using CDP.Core;
using System.IO;

namespace CDP.UnrealTournament2004
{
    public class SettingsViewModel : Core.ViewModelBase
    {
        public DelegateCommand BrowseForExeCommand { get; private set; }
        public string ExeFullPath
        {
            get { return (string)settings["Ut2004ExeFullPath"]; }
        }

        private readonly ISettings settings = ObjectCreator.Get<ISettings>();

        public SettingsViewModel()
        {
            BrowseForExeCommand = new Core.DelegateCommand(BrowseForExeCommandExecute);
        }

        private void BrowseForExeCommandExecute()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Browse for UT2004 executable...",
                Filter = "UT2004.exe|UT2004.exe",
                RestoreDirectory = true
            };

            if (File.Exists((string)settings["Ut2004ExeFullPath"]))
            {
                dialog.InitialDirectory = Path.GetDirectoryName((string)settings["Quake3ExeFUt2004ExeFullPathullPath"]);
            }

            if (dialog.ShowDialog() == true)
            {
                settings["Ut2004ExeFullPath"] = dialog.FileName;
                OnPropertyChanged("ExeFullPath");
            }
        }
    }
}
