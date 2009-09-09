using System;

namespace CDP.ViewModel
{
    public class Header : Core.ViewModelBase
    {
        public DelegateCommand OptionsCommand { get; private set; }
        public DelegateCommand AboutCommand { get; private set; }

        private readonly INavigationService navigationService;

        public Header()
            : this(NavigationService.Instance)
        {
        }

        public Header(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        public override void Initialise()
        {
            OptionsCommand = new DelegateCommand(OptionsCommandCanExecute, OptionsCommandExecute);
            AboutCommand = new DelegateCommand(AboutCommandExecute);
        }

        public override void Initialise(object parameter)
        {
            throw new NotImplementedException();
        }

        public void OptionsCommandExecute()
        {
            navigationService.Navigate(new View.Options(), new ViewModel.Options());
        }

        public bool OptionsCommandCanExecute()
        {
            return (navigationService.CurrentPageTitle != "Options");
        }

        public void AboutCommandExecute()
        {
            System.Windows.MessageBox.Show("about");
        }
    }
}
