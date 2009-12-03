using System;
using CDP.Core;
using CDP.Core.Extensions;

namespace CDP.Gui.ViewModels
{
    internal class DemoError : ViewModelBase
    {
        public string DemoFileName { get; private set; }
        public string Message { get; private set; }
        public DelegateCommand OkCommand { get; private set; }

        private readonly INavigationService navigationService = ObjectCreator.Get<INavigationService>();

        public DemoError(string demoFileName, string errorMessage, Exception exception)
        {
            if (demoFileName == null)
            {
                throw new ArgumentNullException("demoFileName");
            }
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            DemoFileName = demoFileName;
            Message = exception.ToLogString(errorMessage);
            OkCommand = new DelegateCommand(OkCommandExecute);
        }

        public void OkCommandExecute()
        {
            if (navigationService.IsModalWindowActive)
            {
                navigationService.CloseModalWindow(this);
            }
            else
            {
                navigationService.Home();
            }
        }
    }
}
