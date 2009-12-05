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
        private readonly Action onContinue;
        private readonly Action onCancel;

        public DemoWarning(string demoFileName, string message, Exception exception, Action onContinue, Action onCancel)
        {
            if (demoFileName == null)
            {
                throw new ArgumentNullException("demoFileName");
            }
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            DemoFileName = demoFileName;
            Message = exception == null ? message : exception.ToLogString(message);
            this.onContinue = onContinue;
            this.onCancel = onCancel;
            ContinueCommand = new DelegateCommand(ContinueCommandExecute);
            CancelCommand = new DelegateCommand(CancelCommandExecute);
        }

        public void ContinueCommandExecute()
        {
            onContinue();
        }

        public void CancelCommandExecute()
        {
            onCancel();
        }
    }
}
