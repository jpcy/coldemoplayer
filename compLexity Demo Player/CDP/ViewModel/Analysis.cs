using System;
using System.Windows.Controls;

namespace CDP.ViewModel
{
    public class Analysis : Core.ViewModelBase
    {
        public string Header
        {
            get { return string.Format("Analysis of \'{0}\'", demo.Name); }
        }

        public UserControl View { get; private set; }
        public Core.ViewModelBase ViewModel { get; private set; }
        public DelegateCommand BackCommand { get; private set; }

        private readonly INavigationService navigationService = Core.ObjectCreator.Get<INavigationService>();
        private readonly Core.Demo demo;
        private readonly Core.ViewModelBase viewModel;

        public Analysis(Core.Demo demo)
        {
            this.demo = demo;
            viewModel = demo.Handler.CreateAnalysisViewModel(demo);
            BackCommand = new DelegateCommand(BackCommandExecute);
        }

        public override void OnNavigateComplete()
        {
            View = demo.Handler.CreateAnalysisView();
            View.DataContext = viewModel;
            viewModel.OnNavigateComplete();
            OnPropertyChanged("View");
        }

        public void BackCommandExecute()
        {
            navigationService.Home();
        }
    }
}
