using System;

namespace CDP.HalfLifeDemo.Analysis
{
    class ViewModel : Core.ViewModelBase
    {
        private Demo demo;
        public string ServerName { get; private set; }

        public override void Initialise()
        {
            throw new NotImplementedException();
        }

        public override void Initialise(object parameter)
        {
            demo = (Demo)parameter;
            demo.AddMessageCallback<Messages.SvcServerInfo>(MessageServerInfo);
        }

        private void MessageServerInfo(Messages.SvcServerInfo message)
        {
            ServerName = message.ServerName;
            OnPropertyChanged("ServerName");
        }
    }
}
