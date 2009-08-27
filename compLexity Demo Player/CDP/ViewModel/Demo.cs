using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.ViewModel
{
    public class Demo : ViewModelBase
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

                return pathAdapter.Combine(config.ProgramPath, "previews", Data.MapImagesRelativePath);
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

                return pathAdapter.Combine(config.ProgramPath, "overviews", Data.MapImagesRelativePath);
            }
        }
        public DelegateCommand PlayCommand { get; private set; }
        public DelegateCommand AnalyseCommand { get; private set; }

        private readonly IMediator mediator;
        private readonly Core.Config config;
        private readonly Core.Adapters.IPath pathAdapter;

        public Demo()
            : this(Mediator.Instance, Core.Config.Instance, new Core.Adapters.Path())
        {
        }

        public Demo(IMediator mediator, Core.Config config, Core.Adapters.IPath pathAdapter)
        {
            this.mediator = mediator;
            this.config = config;
            this.pathAdapter = pathAdapter;
        }

        public override void Initialise()
        {
            PlayCommand = new DelegateCommand(PlayCommandCanExecute, PlayCommandExecute);
            AnalyseCommand = new DelegateCommand(AnalyseCommandCanExecute, AnalyseCommandExecute);
            mediator.Register<Core.Demo>(Messages.SelectedDemoChanged, SelectedDemoChanged, this);
        }

        public override void Initialise(object parameter)
        {
            throw new NotImplementedException();
        }

        public bool PlayCommandCanExecute()
        {
            return (Data != null && Data.CanPlay);
        }

        public void PlayCommandExecute()
        {
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
