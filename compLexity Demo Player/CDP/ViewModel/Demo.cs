using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.ViewModel
{
    public class Demo : Core.ViewModelBase
    {
        public Core.Demo Data { get; private set; }
        public string MapPreviewFileName
        {
            get
            {
                if (Data == null)
                {
                    return null;
                }

                return fileSystem.PathCombine(settings.ProgramPath, "previews", Data.MapImagesRelativePath);
            }
        }
        public string MapOverviewFileName
        {
            get
            {
                if (Data == null)
                {
                    return null;
                }

                return fileSystem.PathCombine(settings.ProgramPath, "overviews", Data.MapImagesRelativePath);
            }
        }

        public DelegateCommand PlayCommand { get; private set; }
        public DelegateCommand AnalyseCommand { get; private set; }

        private readonly Core.ISettings settings = Core.ObjectCreator.Get<Core.ISettings>();
        private readonly IMediator mediator;
        private readonly Core.IFileSystem fileSystem;
        private Core.DemoManager demoManager;

        public Demo()
            : this(Mediator.Instance, new Core.FileSystem())
        {
        }

        public Demo(IMediator mediator, Core.IFileSystem fileSystem)
        {
            this.mediator = mediator;
            this.fileSystem = fileSystem;
        }

        public override void Initialise()
        {
            throw new NotImplementedException();
        }

        public override void Initialise(object parameter)
        {
            demoManager = (Core.DemoManager)parameter;
            PlayCommand = new DelegateCommand(PlayCommandCanExecute, PlayCommandExecute);
            AnalyseCommand = new DelegateCommand(AnalyseCommandCanExecute, AnalyseCommandExecute);
            mediator.Register<Core.Demo>(Messages.SelectedDemoChanged, SelectedDemoChanged, this);        }

        public bool PlayCommandCanExecute()
        {
            return (Data != null && Data.CanPlay);
        }

        public void PlayCommandExecute()
        {
            Core.Launcher launcher = demoManager.CreateLauncher(Data);

            if (!launcher.Verify())
            {
                System.Windows.MessageBox.Show(launcher.Message);
                return;
            }

            // TODO
        }

        public bool AnalyseCommandCanExecute()
        {
            return (Data != null && Data.CanAnalyse);
        }

        public void AnalyseCommandExecute()
        {
        }

        public void SelectedDemoChanged(Core.Demo demo)
        {
            Data = demo;
            OnPropertyChanged("Data");
            OnPropertyChanged("MapPreviewFileName");
            OnPropertyChanged("MapOverviewFileName");
        }
    }
}
