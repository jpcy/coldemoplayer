using System;
using System.ComponentModel;

namespace CDP
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public abstract void Initialise();
        public abstract void Initialise(object parameter);

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
