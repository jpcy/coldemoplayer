using System;
using System.Linq;
using CDP.Core;

namespace CDP.Gui.ViewModel
{
    public class Address : ViewModelBase
    {
        public string SelectedFolder
        {
            get { return selectedFolder; }
            set { SetFolder(new SetFolderMessageParameters(value, null), true); }
        }

        private readonly IMediator mediator = ObjectCreator.Get<IMediator>();
        private readonly ISettings settings = ObjectCreator.Get<ISettings>();
        private readonly IFileSystem fileSystem = ObjectCreator.Get<IFileSystem>();
        private string selectedFolder = string.Empty;

        public Address()
        {
            mediator.Register<SetFolderMessageParameters>(Messages.SetFolder, SetFolder, this);
        }

        public override void OnNavigateComplete()
        {
            if (Environment.GetCommandLineArgs().Length > 1 && fileSystem.FileExists(Environment.GetCommandLineArgs()[1]))
            {
                // Set folder and selected file based on command line arguments.
                string arg = Environment.GetCommandLineArgs()[1];
                SetFolder(new SetFolderMessageParameters(fileSystem.GetDirectoryName(arg), fileSystem.GetFileName(arg)));
            }
            else if (fileSystem.DirectoryExists((string)settings["LastPath"]))
            {
                // Set folder and selected file to the last path/filename from the last time the program was run.
                string lastFileName = (string)settings["LastFileName"];

                if (!fileSystem.FileExists(fileSystem.PathCombine((string)settings["LastPath"], lastFileName)))
                {
                    lastFileName = null;
                }

                SetFolder(new SetFolderMessageParameters((string)settings["LastPath"], lastFileName));
            }
        }

        public void SetFolder(SetFolderMessageParameters parameters)
        {
            SetFolder(parameters, false);
        }

        public void SetFolder(SetFolderMessageParameters parameters, bool async)
        {
            string lastSelectedFolder = selectedFolder;
            selectedFolder = parameters.Path;

            if (lastSelectedFolder != selectedFolder)
            {
                OnPropertyChanged("SelectedFolder");
                settings["LastPath"] = selectedFolder;
                mediator.Notify<SelectedFolderChangedMessageParameters>(Messages.SelectedFolderChanged, new SelectedFolderChangedMessageParameters(selectedFolder, parameters.FileNameToSelect), async);
            }
        }
    }
}
