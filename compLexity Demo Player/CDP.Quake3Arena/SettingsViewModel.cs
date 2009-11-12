using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.IO;

namespace CDP.Quake3Arena
{
    public class SettingsViewModel : Core.ViewModelBase
    {
        public Core.DelegateCommand BrowseForExeCommand { get; private set; }
        public string ExeFullPath
        {
            get { return (string)settings["Quake3ExeFullPath"]; }
        }

        private readonly Core.ISettings settings = Core.ObjectCreator.Get<Core.ISettings>();

        public SettingsViewModel()
        {
            BrowseForExeCommand = new Core.DelegateCommand(BrowseForExeCommandExecute);
        }

        private void BrowseForExeCommandExecute()
        {
            string initialPath = null;

            if (File.Exists((string)settings["Quake3ExeFullPath"]))
            {
                initialPath = Path.GetDirectoryName((string)settings["Quake3ExeFullPath"]);
            }

            string fileName = BrowseForFile("quake3.exe", initialPath);

            if (!string.IsNullOrEmpty(fileName))
            {
                settings["Quake3ExeFullPath"] = fileName;
                OnPropertyChanged("ExeFullPath");
            }
        }

        private string BrowseForFile(string fileName, string initialPath)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Browse for \"" + fileName + "\"...",
                InitialDirectory = initialPath,
                Filter = fileName + "|" + fileName,
                RestoreDirectory = true
            };

            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }

            return null;
        }
    }
}
