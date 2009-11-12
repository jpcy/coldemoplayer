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
        private readonly string[] executableFileNames;

        public SettingsViewModel(string[] executableFileNames)
        {
            if (executableFileNames == null)
            {
                throw new ArgumentNullException("executableFileNames");
            }

            this.executableFileNames = executableFileNames;
            BrowseForExeCommand = new Core.DelegateCommand(BrowseForExeCommandExecute);
        }

        private void BrowseForExeCommandExecute()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Browse for Quake III Arena executable...",
                Filter = string.Empty,
                RestoreDirectory = true
            };

            if (File.Exists((string)settings["Quake3ExeFullPath"]))
            {
                dialog.InitialDirectory = Path.GetDirectoryName((string)settings["Quake3ExeFullPath"]);
            }

            // OpenFileDialog Filter property does integrity checks everytime it changes, can't use it to build a string incrementally.
            string filter = string.Empty;

            foreach (string fileName in executableFileNames)
            {
                if (fileName != executableFileNames.First())
                {
                    filter += "|";
                }

                filter += fileName + "|" + fileName;
            }

            dialog.Filter = filter;

            if (dialog.ShowDialog() == true)
            {
                settings["Quake3ExeFullPath"] = dialog.FileName;
                OnPropertyChanged("ExeFullPath");
            }
        }
    }
}
