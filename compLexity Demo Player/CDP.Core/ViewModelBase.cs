using System;
using System.ComponentModel;

namespace CDP.Core
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnNavigateComplete() { }

        protected void OnPropertyChanged(string propertyName)
        {
            // Stop the annoying warning.
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
