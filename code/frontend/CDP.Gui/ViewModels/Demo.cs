using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace CDP.Gui.ViewModels
{
    internal class Demo : Core.ViewModelBase
    {
        public Core.Demo Data { get; private set; }
        public string MapThumbnailFileName
        {
            get
            {
                if (Data == null || Data.MapThumbnailRelativePath == null)
                {
                    return null;
                }

                return fileSystem.PathCombine(settings.ProgramPath, "mapthumbnails", Data.MapThumbnailRelativePath);
            }
        }

        public UserControl SettingsView
        {
            get { return Data == null ? null : Data.Plugin.CreateSettingsView(Data); }
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
            navigationService.Navigate(new Views.Play(), new Play(Data));
        }

        public bool AnalyseCommandCanExecute()
        {
            return (Data != null && Data.CanAnalyse);
        }

        public void AnalyseCommandExecute()
        {
            navigationService.Navigate(new Views.AnalysisProgress(), new AnalysisProgress(Data));
        }

        public void SelectedDemoChanged(Core.Demo demo)
        {
            Data = demo;
            OnPropertyChanged("Data");
            OnPropertyChanged("MapThumbnailFileName");
            OnPropertyChanged("SettingsView");
        }
    }
}
