using System;
using CDP.Core;
using CDP.Core.Extensions;

namespace CDP.Gui.ViewModels
{
    internal class DemoInformation : ViewModelBase
    {
        public string DemoFileName { get; private set; }
        public string Message { get; private set; }
        public DelegateCommand OkCommand { get; private set; }

        private readonly INavigationService navigationService = ObjectCreator.Get<INavigationService>();

        public DemoInformation(string demoFileName, string message)
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
            OkCommand = new DelegateCommand(OkCommandExecute);
        }

        public void OkCommandExecute()
        {
            navigationService.Home();
        }
    }
}
