using System;
using System.Collections.Generic;
using System.Text;
using compLexity_Demo_Player;

namespace Server_Browser
{
    public class Favourites : ServerCollection<Server>
    {
        public Favourites(IMainWindow mainWindowInterface)
        {
            this.mainWindowInterface = mainWindowInterface;
        }

        public Server Add(String address)
        {
            Server server = Common.FirstOrDefault<Server>(servers, s => s.Address == address);

            if (server == null)
            {
                server = new Server(mainWindowInterface, address);
                servers.Add(server);
                server.Refresh();
            }

            return server;
        }
    }
}
