using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Threading;
using System.Threading;

namespace CDP.ViewModel
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
                    mediator.Notify<string>(Messages.SelectedFolderChanged, selectedFolder);
                }
            }
        }

        private IMediator mediator;
        private IShell shell;
        private string lastSelectedFolder;
        private string selectedFolder;

        public Address()
            : this(Mediator.Instance, new Shell())
        {
        }

        public Address(IMediator mediator, IShell shell)
        {
            this.mediator = mediator;
            this.shell = shell;
            selectedFolder = "";
        }

        public override void Initialise()
        {
            mediator.Register<string>(Messages.SetSelectedFolder, SetSelectedFolder, this);
        }

        public override void Initialise(object parameter)
        {
        }

        public void SetSelectedFolder(string path)
        {
            SelectedFolder = path;
            OnPropertyChanged("SelectedFolder");
        }
    }
}
