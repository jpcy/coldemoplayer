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
                    mediator.Notify<string>(Messages.SelectedFolderChanged, selectedFolder);
                }
            }
        }

        private readonly IMediator mediator = Core.ObjectCreator.Get<IMediator>();
        private string lastSelectedFolder;
        private string selectedFolder = "";

        public Address()
        {
            mediator.Register<string>(Messages.SetSelectedFolder, SetSelectedFolder, this);
        }

        public void SetSelectedFolder(string path)
        {
            SelectedFolder = path;
            OnPropertyChanged("SelectedFolder");
        }
    }
}
