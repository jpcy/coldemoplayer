using System;
using System.Collections.Generic;
using System.Text;
using System.Net; // IPEndPoint
using System.Net.Sockets; // UdpClient
using System.Threading;
using compLexity_Demo_Player;

namespace Server_Browser
{
    public class TimedUdpClient : UdpClient
    {
        private const Int32 timeout = 2000;

        public TimedUdpClient()
        {
            Client.ReceiveTimeout = timeout;
        }

        public Int32 Send(Byte[] data)
        {
            return base.Send(data, data.Length);
        }

        public new Byte[] Receive(ref IPEndPoint remoteEP)
        {
            try
            {
                return base.Receive(ref remoteEP);
            }
            catch (SocketException)
            {
                return null;
            }
        }
    }
}
