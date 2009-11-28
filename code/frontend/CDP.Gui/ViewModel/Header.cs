using System;
using CDP.Core;

namespace CDP.Gui.ViewModel
{
    public class Header : Core.ViewModelBase
    {
        public DelegateCommand PreferencesCommand { get; private set; }
        public DelegateCommand AboutCommand { get; private set; }

        private readonly INavigationService navigationService = Core.ObjectCreator.Get<INavigationService>();

        public Header()
        {
            PreferencesCommand = new DelegateCommand(OptionsCommandCanExecute, PreferencesCommandExecute);
            AboutCommand = new DelegateCommand(AboutCommandExecute);
        }

        public void PreferencesCommandExecute()
        {
            navigationService.Navigate(new View.Preferences(), new ViewModel.Preferences());
        }

        public bool OptionsCommandCanExecute()
        {
            return (navigationService.CurrentPageTitle != "Preferences");
        }

        public void AboutCommandExecute()
        {
            System.Windows.MessageBox.Show("about");
        }
    }
}
