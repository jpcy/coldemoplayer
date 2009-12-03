using System;
using CDP.Core;
using CDP.Core.Extensions;

namespace CDP.Gui.ViewModels
{
    internal class DemoWarning : ViewModelBase
    {
        public string DemoFileName { get; private set; }
        public string Message { get; private set; }
        public DelegateCommand ContinueCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }

        private readonly INavigationService navigationService = ObjectCreator.Get<INavigationService>();

        public DemoWarning(string demoFileName, string message)
        {
            if (demoFileName == null)
            {
                throw new ArgumentNullException("demoFileName");
            }
            if (message == null)
            {
                throw new ArgumentNullException("exception");
            }

            DemoFileName = demoFileName;
            Message = message;
            ContinueCommand = new DelegateCommand(ContinueCommandExecute);
            CancelCommand = new DelegateCommand(CancelCommandExecute);
        }

        public void ContinueCommandExecute()
        {
            // TODO
            navigationService.Home();
        }

        public void CancelCommandExecute()
        {
            navigationService.Home();
        }
    }
}
