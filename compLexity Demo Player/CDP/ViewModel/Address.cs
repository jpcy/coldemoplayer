using System;

namespace CDP.ViewModel
{
    public class Address : Core.ViewModelBase
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
                    mediator.Notify<string>(Messages.SelectedFolderChanged, selectedFolder);
                }
            }
        }

        private readonly IMediator mediator = Core.ObjectCreator.Get<IMediator>();
        private readonly Core.ISettings settings = Core.ObjectCreator.Get<Core.ISettings>();
        private string lastSelectedFolder;
        private string selectedFolder = "";

        public Address()
        {
            mediator.Register<string>(Messages.SetSelectedFolder, SetSelectedFolder, this);
        }

        public override void OnNavigateComplete()
        {
            SetSelectedFolder((string)settings["LastPath"]);
        }

        public void SetSelectedFolder(string path)
        {
            SelectedFolder = path;
            OnPropertyChanged("SelectedFolder");
        }
    }
}
