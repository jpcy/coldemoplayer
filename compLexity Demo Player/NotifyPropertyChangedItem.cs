using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel; // INotifyPropertyChanged

namespace compLexity_Demo_Player
{
    public class NotifyPropertyChangedItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
