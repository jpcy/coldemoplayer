using CDP.Core;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;

namespace CDP.Gui.ViewModel
{
    internal class Preferences : Core.ViewModelBase
    {
        internal class FileAssociation
        {
            public bool IsSelected { get; set; }
            public string Extension { get; private set; }
            public string PluginNames { get; private set; }

            public FileAssociation(bool isSelected, string extension, string[] pluginNames)
            {
                IsSelected = isSelected;
                Extension = extension.Replace("_", "__");

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < pluginNames.Length; i++)
                {
                    sb.Append(i == 0 ? string.Empty : ", ");
                    sb.Append(pluginNames[i]);
                }

                PluginNames = sb.ToString();
            }
        }

        public Header Header { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }
        public DelegateCommand ApplyCommand { get; private set; }
        public ObservableCollection<FileAssociation> FileAssociations { get; private set; }

        private readonly INavigationService navigationService = ObjectCreator.Get<INavigationService>();
        private readonly ISettings settings = ObjectCreator.Get<ISettings>();

        public Preferences()
        {
            Header = new Header();
            CancelCommand = new DelegateCommand(CancelCommandExecute);
            ApplyCommand = new DelegateCommand(ApplyCommandExecute);
            FileAssociations = new ObservableCollection<FileAssociation>();
        }

        public override void OnNavigateComplete()
        {
            IDemoManager demoManager = ObjectCreator.Get<IDemoManager>();
            string[] extensions = demoManager.ValidDemoExtensions();

            foreach (string extension in extensions)
            {
                FileAssociations.Add(new FileAssociation((bool)settings[settings.FileAssociationSettingPrefix + extension], extension, demoManager.GetDemoHandlerNames(extension)));
            }
        }

        public void CancelCommandExecute()
        {
            navigationService.Home();
        }

        public void ApplyCommandExecute()
        {
            navigationService.Home();
        }
    }
}
