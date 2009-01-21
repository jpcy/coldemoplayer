using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace compLexity_Demo_Player
{
    public abstract class Demo
    {
        public enum StatusEnum
        {
            Ok,
            CorruptDirEntries
        }

        public enum EngineEnum
        {
            HalfLife,
            HalfLifeSteam,
            Source
        }

        public enum PerspectiveEnum
        {
            Pov,
            Hltv,
            SourceTv
        }

        protected String name;
        protected String fileFullPath;

        protected StatusEnum status;
        protected EngineEnum engineType;
        protected PerspectiveEnum perspective;
        
        protected UInt32 demoProtocol;
        protected UInt32 networkProtocol;
        protected String clientDllChecksum;
        protected String serverName;
        protected String recorderName;
        protected String mapName;
        protected String gameFolderName;
        protected UInt32 mapChecksum;
        protected Single durationInSeconds;
        protected Byte maxClients;
        protected Int32 buildNumber;

        protected IMainWindow mainWindowInterface;
        protected IDemoListView demoListViewInterface;
        protected IProgressWindow writeProgressWindowInterface;

        #region Properties
        public String Name
        {
            get
            {
                return name;
            }
        }

        public String FileFullPath
        {
            get
            {
                return fileFullPath;
            }
        }

        public StatusEnum Status
        {
            get
            {
                return status;
            }
        }

        public EngineEnum Engine
        {
            get
            {
                return engineType;
            }
        }

        public virtual String EngineName
        {
            get
            {
                return "";
            }
        }

        public PerspectiveEnum Perspective
        {
            get
            {
                return perspective;
            }
        }

        public String PerspectiveString
        {
            get
            {
                switch (perspective)
                {
                    case PerspectiveEnum.Pov:
                        return "POV";

                    case PerspectiveEnum.Hltv:
                        return "HLTV";

                    case PerspectiveEnum.SourceTv:
                        return "Source TV";
                }

                return "Unknown";
            }
        }

        public UInt32 DemoProtocol
        {
            get
            {
                return demoProtocol;
            }
        }

        public UInt32 NetworkProtocol
        {
            get
            {
                return networkProtocol;
            }
        }

        public String ServerName
        {
            get
            {
                return serverName;
            }
        }

        public String RecorderName
        {
            get
            {
                return recorderName;
            }
        }

        public String MapName
        {
            get
            {
                return mapName;
            }
        }

        public String GameFolderName
        {
            get
            {
                return gameFolderName;
            }
        }

        public UInt32 MapChecksum
        {
            get
            {
                return mapChecksum;
            }
        }

        public Single DurationInSeconds
        {
            get
            {
                return durationInSeconds;
            }

            set
            {
                durationInSeconds = value;
            }
        }

        public Byte MaxClients
        {
            get
            {
                return maxClients;
            }

            set
            {
                maxClients = value;
            }
        }

        public Int32 BuildNumber
        {
            get
            {
                return buildNumber;
            }
        }

        public SteamGameInfo SteamGameInfo
        {
            get
            {
                return Steam.GetGameInfo((engineType == EngineEnum.Source), gameFolderName);
            }
        }

        public String GameName
        {
            get
            {
                // try to get game name from config\steam.xml
                SteamGameInfo info = this.SteamGameInfo;

                if (info == null)
                {
                    return "Unknown (" + gameFolderName + ")";
                }

                // try to get game version from config\*engine*\*game folder*.xml
                String engineName = "goldsrc";

                if (engineType == EngineEnum.Source)
                {
                    engineName = "source";
                }

                GameConfig gameConfig = GameConfigList.Find(gameFolderName, engineName);

                if (gameConfig != null)
                {
                    String version = gameConfig.FindVersion(clientDllChecksum);

                    if (version != null)
                    {
                        return info.GameName + " " + version;
                    }
                }

                return info.GameName;
            }
        }

        #endregion

        public Demo()
        {
        }

        /// <summary>
        /// Starts reading the demo in a new thread.
        /// </summary>
        public Thread Read(IMainWindow mainWindowInterface, IDemoListView demoListViewInterface)
        {
            this.mainWindowInterface = mainWindowInterface;
            this.demoListViewInterface = demoListViewInterface;

            // calculate file full path and name
            //fileFullPath = Path.GetFullPath(fileFullPath); // something to do with 8.3?
            name = Path.GetFileNameWithoutExtension(fileFullPath);

            // start the reading thread
            Thread thread = new Thread(new ThreadStart(ReadingThread));
            thread.Name = "Reading Demo";
            thread.Start();

            return thread;
        }

        /// <summary>
        /// Starts writing the demo in a new thread
        /// </summary>
        public Thread Write(IProgressWindow writeProgressWindowInterface)
        {
            this.writeProgressWindowInterface = writeProgressWindowInterface;
            return new Thread(new ParameterizedThreadStart(WritingThread)) { Name = "Writing Demo" };
        }

        protected abstract void ReadingThread();
        protected abstract void WritingThread(object _destinationPath);
    }
}
