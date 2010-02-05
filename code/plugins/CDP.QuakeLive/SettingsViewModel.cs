using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CDP.Core;
using Microsoft.Win32;

namespace CDP.QuakeLive
{
    internal class SettingsViewModel : ViewModelBase
    {
        public string ExeFullPath
        {
            get { return (string)settings["QuakeLiveExeFullPath"]; }
            set { settings["QuakeLiveExeFullPath"] = value; }
        }

        public DelegateCommand BrowseForExeCommand { get; private set; }

        private readonly ISettings settings = ObjectCreator.Get<ISettings>();
        private readonly IFileSystem fileSystem = ObjectCreator.Get<IFileSystem>();
        private readonly Executable[] executables;

        public SettingsViewModel(Executable[] executables)
        {
            if (executables == null)
            {
                throw new ArgumentNullException("executables");
            }

            this.executables = executables;
            BrowseForExeCommand = new Core.DelegateCommand(BrowseForExeCommandExecute);
        }

        private void BrowseForExeCommandExecute()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = Strings.BrowseForExeDialogTitle,
                Filter = string.Empty,
                RestoreDirectory = true
            };

            if (fileSystem.FileExists(ExeFullPath))
            {
                dialog.InitialDirectory = fileSystem.GetDirectoryName(ExeFullPath);
            }

            // OpenFileDialog Filter property does integrity checks everytime it changes, can't use it to build a string incrementally.
            string filter = string.Empty;

            foreach (Executable exe in executables)
            {
                if (exe != executables.First())
                {
                    filter += "|";
                }

                filter += exe.Name + "|" + exe.FileName;
            }

            dialog.Filter = filter;

            if (dialog.ShowDialog() == true)
            {
                ExeFullPath = dialog.FileName;
                OnPropertyChanged("ExeFullPath");
            }
        }
    }
}
