using System;
using CDP.Core;

namespace CDP.Gui.ViewModel
{
    public class Address : ViewModelBase
    {
        public string SelectedFolder
        {
            get { return selectedFolder; }
            set
            {
                lastSelectedFolder = selectedFolder;
                selectedFolder = value;

                if (lastSelectedFolder != selectedFolder)
                {
                    settings["LastPath"] = selectedFolder;
                    mediator.Notify<string>(Messages.SelectedFolderChanged, selectedFolder, true);
                }
            }
        }

        private readonly IMediator mediator = ObjectCreator.Get<IMediator>();
        private readonly ISettings settings = ObjectCreator.Get<ISettings>();
        private readonly IFileSystem fileSystem = ObjectCreator.Get<IFileSystem>();
        private string lastSelectedFolder;
        private string selectedFolder = string.Empty;

        public Address()
        {
            mediator.Register<string>(Messages.SetSelectedFolder, SetSelectedFolder, this);
        }

        public override void OnNavigateComplete()
        {
            if (fileSystem.DirectoryExists((string)settings["LastPath"]))
            {
                SetSelectedFolder((string)settings["LastPath"]);
            }
        }

        public void SetSelectedFolder(string path)
        {
            SelectedFolder = path;
            OnPropertyChanged("SelectedFolder");
        }
    }
}
