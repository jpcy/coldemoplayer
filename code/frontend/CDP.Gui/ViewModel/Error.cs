using System;
using CDP.Core;
using CDP.Core.Extensions;

namespace CDP.Gui.ViewModel
{
    class Error : Core.ViewModelBase
    {
        public string Message { get; private set; }
        public string Exception { get; private set; }
        public DelegateCommand OkCommand { get; private set; }

        private readonly INavigationService navigationService = Core.ObjectCreator.Get<INavigationService>();

        public Error(string errorMessage, Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            Message = errorMessage ?? exception.Message;

            if (Message.Contains("\n"))
            {
                Message = "Error";
            }

            Exception = exception.ToLogString(errorMessage);
            OkCommand = new DelegateCommand(OkCommandExecute);
        }

        public void OkCommandExecute()
        {
            navigationService.Home();
        }
    }
}
