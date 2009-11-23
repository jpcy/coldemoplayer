using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.Gui.ViewModel
{
    public class Demo : Core.ViewModelBase
    {
        public Core.Demo Data { get; private set; }
        public string MapPreviewFileName
        {
            get
            {
                if (Data == null || Data.MapImagesRelativePath == null)
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
                if (Data == null || Data.MapImagesRelativePath == null)
                {
                    return null;
                }

                return fileSystem.PathCombine(settings.ProgramPath, "overviews", Data.MapImagesRelativePath);
            }
        }

        public Core.DelegateCommand PlayCommand { get; private set; }
        public Core.DelegateCommand AnalyseCommand { get; private set; }

        private readonly Core.ISettings settings = Core.ObjectCreator.Get<Core.ISettings>();
        private readonly IMediator mediator = Core.ObjectCreator.Get<IMediator>();
        private readonly Core.IFileSystem fileSystem = Core.ObjectCreator.Get<Core.IFileSystem>();
        private readonly INavigationService navigationService = Core.ObjectCreator.Get<INavigationService>();

        public Demo()
        {
            PlayCommand = new Core.DelegateCommand(PlayCommandCanExecute, PlayCommandExecute);
            AnalyseCommand = new Core.DelegateCommand(AnalyseCommandCanExecute, AnalyseCommandExecute);
            mediator.Register<Core.Demo>(Messages.SelectedDemoChanged, SelectedDemoChanged, this);
        }

        public bool PlayCommandCanExecute()
        {
            return (Data != null && Data.CanPlay);
        }

        public void PlayCommandExecute()
        {
            navigationService.Navigate(new View.Play(), new Play(Data));
        }

        public bool AnalyseCommandCanExecute()
        {
            return (Data != null && Data.CanAnalyse);
        }

        public void AnalyseCommandExecute()
        {
            navigationService.Navigate(new View.AnalysisProgress(), new AnalysisProgress(Data));
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
