using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel; // ObservableCollection
using System.Threading;
using System.Windows.Threading;
using System.Reflection; // PropertyInfo
using System.Diagnostics; // Assert
using compLexity_Demo_Player;

namespace Server_Browser
{
    public class ServerCollection<T> where T:Server
    {
        public ObservableCollection<T> Servers
        {
            get
            {
                return servers;
            }
        }

        protected ObservableCollection<T> servers = new ObservableCollection<T>();
        protected IMainWindow mainWindowInterface;

        public virtual void RefreshAll()
        {
            foreach (T server in servers)
            {
                server.Refresh();
            }
        }

        public void RemoveServer(T server)
        {
            if (server == null)
            {
                return;
            }

            server.AbortRefresh();
            servers.Remove(server);
        }

        public virtual void AbortAllThreads()
        {
            foreach (T server in servers)
            {
                server.AbortRefresh();
            }
        }
    }
}
