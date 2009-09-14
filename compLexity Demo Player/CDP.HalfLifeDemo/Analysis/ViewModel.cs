using System;

namespace CDP.HalfLifeDemo.Analysis
{
    class ViewModel : Core.ViewModelBase
    {
        private Demo demo;
        public string ServerName { get; private set; }

        public ViewModel(Core.Demo demo)
        {
            this.demo = (Demo)demo;
            this.demo.AddMessageCallback<Messages.SvcServerInfo>(MessageServerInfo);
        }

        private void MessageServerInfo(Messages.SvcServerInfo message)
        {
            ServerName = message.ServerName;
            OnPropertyChanged("ServerName");
        }
    }
}
