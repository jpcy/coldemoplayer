using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using CDP.Core;

namespace CDP.Gui.ViewModels
{
    internal class Preferences : Core.ViewModelBase
    {
        public class FileAssociation
        {
            public bool IsSelected { get; set; }
            public string ExtensionText { get; private set; }
            public string Extension { get; private set; }
            public string PluginNames { get; private set; }

            public FileAssociation(bool isSelected, string extension, string[] pluginNames)
            {
                IsSelected = isSelected;
                Extension = extension;
                ExtensionText = Extension.Replace("_", "__");

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < pluginNames.Length; i++)
                {
                    sb.Append(i == 0 ? string.Empty : ", ");
                    sb.Append(pluginNames[i]);
                }

                PluginNames = sb.ToString();
            }
        }

        public enum ShellCommands
        {
            Open,
            Play
        }

        public Header Header { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }
        public DelegateCommand ApplyCommand { get; private set; }
        public ObservableCollection<FileAssociation> FileAssociations { get; private set; }

        public bool OpenShellCommand
        {
            get { return shellCommand == ShellCommands.Open; }
            set { shellCommand = value == true ? ShellCommands.Open : ShellCommands.Play; }
        }

        public bool PlayShellCommand
        {
            get { return shellCommand == ShellCommands.Play; }
            set { shellCommand = value == true ? ShellCommands.Play : ShellCommands.Open; }
        }

        private ShellCommands shellCommand;
        private readonly INavigationService navigationService = ObjectCreator.Get<INavigationService>();
        private readonly ISettings settings = ObjectCreator.Get<ISettings>();

        public Preferences()
        {
            Header = new Header();
            CancelCommand = new DelegateCommand(CancelCommandExecute);
            ApplyCommand = new DelegateCommand(ApplyCommandExecute);
            FileAssociations = new ObservableCollection<FileAssociation>();
            shellCommand = (string)settings["DefaultShellCommand"] == "open" ? ShellCommands.Open : ShellCommands.Play;
        }

        public override void OnNavigateComplete()
        {
            IDemoManager demoManager = ObjectCreator.Get<IDemoManager>();
            string[] extensions = demoManager.GetAllPluginFileExtensions();

            foreach (string extension in extensions)
            {
                FileAssociations.Add(new FileAssociation((bool)settings[settings.FileAssociationSettingPrefix + extension], extension, demoManager.GetPluginNames(extension)));
            }
        }

        public void CancelCommandExecute()
        {
            navigationService.Home();
        }

        public void ApplyCommandExecute()
        {
            settings["DefaultShellCommand"] = shellCommand.ToString().ToLower();

            string[] selectedExtensions = (from fa in FileAssociations
                                           where fa.IsSelected
                                           select fa.Extension).ToArray();

            IFileAssociation fileAssociation = ObjectCreator.Get<IFileAssociation>();
            if (fileAssociation.Associate(selectedExtensions, (string)settings["DefaultShellCommand"]))
            {
                // TODO: friendly message that this may cause the screen to flicker (doesn't seem to affect Windows 7, definitely occurs in XP).
                fileAssociation.RefreshIcons();
            }

            // Save file association preferences to settings.
            foreach (FileAssociation fa in FileAssociations)
            {
                settings[settings.FileAssociationSettingPrefix + fa.Extension] = fa.IsSelected;
            }

            navigationService.Home();
        }
    }
}
