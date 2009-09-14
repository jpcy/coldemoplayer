using System;

namespace CDP.ViewModel
{
    public class Progress : Core.ViewModelBase
    {
        public event EventHandler CancelEvent;

        private int _value = 0;
        public int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged("Value");
            }
        }

        public DelegateCommand CancelCommand { get; private set; }

        public Progress()
        {
            CancelCommand = new DelegateCommand(CancelCommandExecute);
        }

        public void CancelCommandExecute()
        {
            OnCancel();
        }

        private void OnCancel()
        {
            if (CancelEvent != null)
            {
                CancelEvent(this, EventArgs.Empty);
            }
        }
    }
}
