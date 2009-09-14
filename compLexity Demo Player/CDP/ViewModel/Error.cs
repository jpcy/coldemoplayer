using System;

namespace CDP.ViewModel
{
    class Error : Core.ViewModelBase
    {
        public string Text { get; private set; }
        public DelegateCommand OkCommand { get; private set; }

        private readonly INavigationService navigationService = Core.ObjectCreator.Get<INavigationService>();

        public Error(string errorMessage, Exception exception)
        {
            OkCommand = new DelegateCommand(OkCommandExecute);
            Text = "";

            if (!string.IsNullOrEmpty(errorMessage))
            {
                Text = errorMessage + "\r\n\r\n";
            }

            if (exception != null)
            {
                Action<Exception> appendExceptionInfo = null;

                appendExceptionInfo = e =>
                {
                    if (e.InnerException != null)
                    {
                        appendExceptionInfo(e.InnerException);
                    }

                    Text += e.Message + "\r\n\r\n" + "Inner Exception: " + (e != exception).ToString() + "\r\n\r\n" + "Source: " + e.Source + "\r\n\r\n" + "Type: " + e.GetType().ToString() + "\r\n\r\n" + e.StackTrace;
                };

                appendExceptionInfo(exception);
            }
        }

        public void OkCommandExecute()
        {
            navigationService.Home();
        }
    }
}
