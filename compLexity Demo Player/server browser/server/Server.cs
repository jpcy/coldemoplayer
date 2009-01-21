using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections.ObjectModel;
using System.Net; // IPEndPoint
using System.IO;
using System.Diagnostics;
using compLexity_Demo_Player;

namespace Server_Browser
{
    public class Server : NotifyPropertyChangedItem
    {
        public enum StateEnum
        {
            Idle,
            Querying,
            Succeeded,
            Failed
        }

        #region Properties
        public StateEnum State
        {
            get
            {
                return state;
            }

            set
            {
                state = value;
                OnPropertyChanged("StateImagePath");
            }
        }

        public String StateImagePath
        {
            get
            {
                return String.Format("..\\data\\status_{0}.png", Enum.GetName(typeof(StateEnum), state).ToLower());
            }
        }

        public String Address 
        {
            get
            {
                return address;
            }

            set
            {
                address = value;
                OnPropertyChanged("Address");
            }
        }

        public String Title 
        {
            get
            {
                return title;
            }

            set
            {
                title = value;
                OnPropertyChanged("Title");
            }
        }

        public Int32? Ping
        {
            get
            {
                return ping;
            }

            set
            {
                ping = value;
                OnPropertyChanged("Ping");
            }
        }


        public String Game
        {
            get
            {
                return game;
            }

            set
            {
                game = value;
                OnPropertyChanged("Game");
            }
        }

        public String Map
        {
            get
            {
                return map;
            }

            set
            {
                map = value;
                OnPropertyChanged("Map");
            }
        }

        public String Spectators
        {
            get
            {
                if (numSpectators == null || maxSpectators == null)
                {
                    return null;
                }

                return String.Format("{0}/{1}", numSpectators, maxSpectators);
            }
        }

        public Byte? NumSpectators
        {
            get
            {
                return numSpectators;
            }

            set
            {
                numSpectators = value;
                OnPropertyChanged("Spectators");
            }
        }

        public Byte? MaxSpectators
        {
            get
            {
                return maxSpectators;
            }

            set
            {
                maxSpectators = value;
                OnPropertyChanged("Spectators");
            }
        }

        public Boolean? PasswordProtected
        {
            get
            {
                return passwordProtected;
            }

            set
            {
                passwordProtected = value;
                OnPropertyChanged("PasswordProtected");
            }
        }

        public ObservableCollection<Player> Players
        {
            get
            {
                OnPropertyChanged("NumPlayers"); // hey, it works...
                return players;
            }

            set
            {
                players = value;
                OnPropertyChanged("Players");
            }
        }

        public Int32? NumPlayers
        {
            get
            {
                if (players == null)
                {
                    return null;
                }

                return players.Count;
            }
        }

        // not used for data binding
        public String GameFolder { get; set; }
        public Boolean SourceEngine { get; set; }
        public Thread Thread { get; set; }
        #endregion

        private StateEnum state = StateEnum.Idle;
        private String address;
        private String title = "Idle";
        private Int32? ping;
        private String game;
        private String map;
        private Byte? numSpectators;
        private Byte? maxSpectators;
        private Boolean? passwordProtected;
        private ObservableCollection<Player> players;

        private Boolean abortRefresh = false;

        private const Byte S2C_CHALLENGE = 0x41; // 'A'
        private const Byte S2C_PLAYER = 0x44;
        private const Byte S2C_INFO_SOURCE = 0x49;
        private const Byte A2S_INFO = 0x54; // 'T'
        private const Byte A2S_PLAYER = 0x55; // 'U'
        private const Byte A2S_SERVERQUERY_GETCHALLENGE = 0x57; // 'W'
        private const Byte S2C_INFO_GOLDSRC = 0x6D;

        protected IMainWindow mainWindowInterface;

        public Server()
        {
        }

        public Server(IMainWindow mainWindowInterface, String address)
        {
            this.mainWindowInterface = mainWindowInterface;
            this.address = address;
        }

        /// <summary>
        /// Aborts the refresh thread, blocking until the thread stops.
        /// </summary>
        public void AbortRefresh()
        {
            if (Thread != null && Thread.IsAlive)
            {
                abortRefresh = true;
            }
        }

        public void Refresh()
        {
            AbortRefresh();

            if (Players != null)
            {
                Players.Clear();
            }

            Thread = new Thread(new ThreadStart(RefreshThread));
            Thread.Start();
        }

        private void RefreshThread()
        {
            using (TimedUdpClient client = new TimedUdpClient())
            {
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);

                mainWindowInterface.SetServerProperty(this, "Title", "Connecting to Server");
                mainWindowInterface.SetServerProperty(this, "State", StateEnum.Querying);

                // reset properties
                mainWindowInterface.SetServerProperty(this, "Ping", null);
                mainWindowInterface.SetServerProperty(this, "Game", null);
                mainWindowInterface.SetServerProperty(this, "Map", null);
                mainWindowInterface.SetServerProperty(this, "NumSpectators", null);
                mainWindowInterface.SetServerProperty(this, "MaxSpectators", null);
                mainWindowInterface.SetServerProperty(this, "PasswordProtected", null);

                try
                {
                    DateTime connectTime = DateTime.Now;

                    // attempt to connect
                    Int32 colonIndex = address.IndexOf(':');
                    // what's the timeout???
                    client.Connect(address.Remove(colonIndex), Convert.ToInt32(address.Substring(colonIndex + 1)));

                    // update title as "connected"
                    mainWindowInterface.SetServerProperty(this, "Title", "Connected");

                    if (abortRefresh)
                    {
                        Common.AbortThread(Thread);
                    }

                    // send A2S_INFO
                    // -1 (int), A2S_INFO, "Source Engine Query" (string) 
                    DateTime sendTime = DateTime.Now;
                    client.Send(new Byte[] { 0xFF, 0xFF, 0xFF, 0xFF, A2S_INFO, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x20, 0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65, 0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 0x00 });

                    // receive A2S_INFO reply
                    Byte[] infoReply = client.Receive(ref ipEndPoint);
                    if (infoReply == null)
                    {
                        throw new ApplicationException("Server not responding.");
                    }

                    // calculate ping
                    TimeSpan ping = DateTime.Now - connectTime;
                    mainWindowInterface.SetServerProperty(this, "Ping", ping.Milliseconds);

                    // parse A2S_INFO reply
                    ParseInfoQueryReply(infoReply);

                    if (client.Available > 0) // fuuuuuuckkkk yoooooouuu Vaaaaaaalllvvvveeee
                    {
                        client.Receive(ref ipEndPoint);
                    }

                    // send A2S_SERVERQUERY_GETCHALLENGE
                    // -1 (int), A2S_SERVERQUERY_GETCHALLENGE
                    /*client.Send(new Byte[] { 0xFF, 0xFF, 0xFF, 0xFF, A2S_SERVERQUERY_GETCHALLENGE });

                    // receive and parse A2S_SERVERQUERY_GETCHALLENGE reply
                    Byte[] serverQueryGetChallengeReply = client.Receive(ref ipEndPoint);
                    if (serverQueryGetChallengeReply == null)
                    {
                        throw new ApplicationException("Server ignored challenge.");
                    }*/

                    //Int32 challengeNumber = ParseServerQueryGetChallengeReply(serverQueryGetChallengeReply);
                    Int32 challengeNumber = -1;

                    while (true)
                    {
                        if (abortRefresh)
                        {
                            Common.AbortThread(Thread);
                        }

                        // send A2S_PLAYER
                        BitWriter bitWriter = new BitWriter();
                        bitWriter.WriteInt32(-1);
                        bitWriter.WriteByte(A2S_PLAYER);
                        bitWriter.WriteInt32(challengeNumber);
                        client.Send(bitWriter.Data);

                        // receive and parse A2S_PLAYER reply
                        Byte[] playerReply = client.Receive(ref ipEndPoint);
                        if (playerReply == null)
                        {
                            if (challengeNumber != -1)
                            {
                                // oh, it's a sourcetv server and valve are too incompetent to implement s2c_player
                                break;
                            }

                            throw new ApplicationException("Player query failed.");
                        }

                        // check for a challenge number (source servers)
                        BitBuffer bitBuffer = new BitBuffer(playerReply);

                        if (bitBuffer.ReadInt32() != -1)
                        {
                            throw new ApplicationException("Bad A2S_PLAYER reply");
                        }

                        Byte type = bitBuffer.ReadByte();

                        if (type == S2C_CHALLENGE)
                        {
                            challengeNumber = bitBuffer.ReadInt32();
                            continue;
                        }
                        else if (type == S2C_PLAYER)
                        {
                            ParsePlayerQueryReply(bitBuffer);
                        }
                        else
                        {
                            throw new ApplicationException(String.Format("Bad A2S_PLAYER type: {0}", type));
                        }

                        break;
                    }

                    mainWindowInterface.SetServerProperty(this, "State", StateEnum.Succeeded);
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    if (!abortRefresh)
                    {
                        mainWindowInterface.SetServerProperty(this, "Title", ex.Message);
                        mainWindowInterface.SetServerProperty(this, "State", StateEnum.Failed);
                    }
                }
                finally
                {
                    client.Close();
                }
            }
        }

        /// <summary>
        /// Parses S2C_INFO.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="si"></param>
        private void ParseInfoQueryReply(Byte[] data)
        {
            BitBuffer bitBuffer = new BitBuffer(data);

            if (bitBuffer.ReadInt32() != -1)
            {
                throw new ApplicationException("Bad A2S_INFO reply");
            }

            // read reply type
            Byte type = bitBuffer.ReadByte();
            Boolean sourceEngine;

            if (type == S2C_INFO_SOURCE)
            {
                sourceEngine = true;
            }
            else if (type == S2C_INFO_GOLDSRC)
            {
                sourceEngine = false;
            }
            else
            {
                throw new ApplicationException(String.Format("Bad A2S_INFO type: {0}", type));
            }

            mainWindowInterface.SetServerProperty(this, "SourceEngine", sourceEngine);

            // read the rest
            if (!sourceEngine)
            {
                bitBuffer.ReadString();
                //si.Address = bitBuffer.ReadString(); // resolved hostname
            }
            else
            {
                bitBuffer.SeekBytes(1); // network version
            }

            mainWindowInterface.SetServerProperty(this, "Title", bitBuffer.ReadString()); // server name
            mainWindowInterface.SetServerProperty(this, "Map", bitBuffer.ReadString()); // map
            String gameFolder = bitBuffer.ReadString();
            mainWindowInterface.SetServerProperty(this, "GameFolder", gameFolder); // game folder
            String game = bitBuffer.ReadString(); // game name

            if (sourceEngine)
            {
                bitBuffer.SeekBytes(2); // app id
            }

            mainWindowInterface.SetServerProperty(this, "NumSpectators", bitBuffer.ReadByte()); // num spectators
            mainWindowInterface.SetServerProperty(this, "MaxSpectators", bitBuffer.ReadByte()); // max spectators
            bitBuffer.SeekBytes(1); // goldsrc: network version, source: num bots
            bitBuffer.SeekBytes(1); // dedicated
            bitBuffer.SeekBytes(1); // os
            mainWindowInterface.SetServerProperty(this, "PasswordProtected", (bitBuffer.ReadByte() == 1));

            // determine game name
            SteamGameInfo sgi = Steam.GetGameInfo(sourceEngine, gameFolder);

            if (sgi == null)
            {
                mainWindowInterface.SetServerProperty(this, "Game", String.Format("Unknown ({0})", game));
            }
            else
            {
                mainWindowInterface.SetServerProperty(this, "Game", sgi.GameName);
            }
        }

        /// <summary>
        /// Parses S2C_CHALLENGE and returns the challenge number.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private Int32 ParseServerQueryGetChallengeReply(Byte[] data)
        {
            BitBuffer bitBuffer = new BitBuffer(data);

            if (bitBuffer.ReadInt32() != -1)
            {
                throw new ApplicationException("Bad A2S_SERVERQUERY_GETCHALLENGE reply");
            }

            Byte type = bitBuffer.ReadByte();

            if (type != S2C_CHALLENGE)
            {
                throw new ApplicationException(String.Format("Bad A2S_SERVERQUERY_GETCHALLENGE type: {0}", type));
            }

            return bitBuffer.ReadInt32();
        }

        /// <summary>
        /// Parses S2C_PLAYER.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="si"></param>
        private void ParsePlayerQueryReply(BitBuffer bitBuffer)
        {
            Int32 nPlayers = bitBuffer.ReadByte();

            for (Int32 i = 0; i < nPlayers; i++)
            {
                bitBuffer.SeekBytes(1); // skip index

                Player player = new Player();
                player.Name = bitBuffer.ReadString();
                player.Score = bitBuffer.ReadInt32();
                Single time = bitBuffer.ReadSingle();
                player.Time = (time == -1 ? "BOT" : Common.DurationString(time));

                mainWindowInterface.AddPlayerToServer(this, player);
            }
        }
    }
}
