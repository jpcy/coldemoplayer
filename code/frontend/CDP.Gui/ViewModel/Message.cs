using System;
using CDP.Core;
using CDP.Core.Extensions;

namespace CDP.Gui.ViewModel
{
    internal class Message : Core.ViewModelBase
    {
        public string Text { get; private set; }
        public string Exception { get; private set; }
        public DelegateCommand OkCommand { get; private set; }

        private readonly INavigationService navigationService = Core.ObjectCreator.Get<INavigationService>();

        public Message(string errorMessage, Exception exception)
        {
            if (errorMessage == null && exception == null)
            {
                throw new ApplicationException("errorMessage or exception can be null, but not both.");
            }

            if (exception == null)
            {
                Text = errorMessage;
            }
            else
            {
                Text = exception.ToLogString(errorMessage);
            }

            OkCommand = new DelegateCommand(OkCommandExecute);
        }

        public void OkCommandExecute()
        {
            navigationService.CloseModalWindow(this);
        }
    }
}
