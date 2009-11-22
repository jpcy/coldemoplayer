using System;
using CDP.Core;

namespace CDP.ViewModel
{
    public class Header : Core.ViewModelBase
    {
        public DelegateCommand OptionsCommand { get; private set; }
        public DelegateCommand AboutCommand { get; private set; }

        private readonly INavigationService navigationService = Core.ObjectCreator.Get<INavigationService>();

        public Header()
        {
            OptionsCommand = new DelegateCommand(OptionsCommandCanExecute, OptionsCommandExecute);
            AboutCommand = new DelegateCommand(AboutCommandExecute);
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
