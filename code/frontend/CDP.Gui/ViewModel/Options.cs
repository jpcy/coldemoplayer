using CDP.Core;

namespace CDP.Gui.ViewModel
{
    public class Options : Core.ViewModelBase
    {
        public Header Header { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }
        public DelegateCommand ApplyCommand { get; private set; }

        private readonly INavigationService navigationService = ObjectCreator.Get<INavigationService>();

        public Options()
        {
            Header = new Header();
            CancelCommand = new DelegateCommand(CancelCommandExecute);
            ApplyCommand = new DelegateCommand(ApplyCommandExecute);
        }

        public void CancelCommandExecute()
        {
            navigationService.Home();
        }

        public void ApplyCommandExecute()
        {
            navigationService.Home();
        }
    }
}
