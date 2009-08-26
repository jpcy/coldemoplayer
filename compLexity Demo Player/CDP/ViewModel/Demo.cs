using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.ViewModel
{
    public class Demo : ViewModelBase
    {
        public Core.Demo Data { get; private set; }
        public DelegateCommand PlayCommand { get; private set; }
        public DelegateCommand AnalyseCommand { get; private set; }

        private readonly IMediator mediator;

        public Demo()
            : this(Mediator.Instance)
        {
        }

        public Demo(IMediator mediator)
        {
            this.mediator = mediator;
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
        }
    }
}
