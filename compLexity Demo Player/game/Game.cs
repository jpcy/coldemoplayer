using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Media;
using System.Collections.Specialized;
using JsonExSerializer;

namespace compLexity_Demo_Player
{
    public class Game
    {
        public enum Engines
        {
            HalfLife,
            Source
        }

        public Engines Engine { get; protected set; }
        public Int32 AppId { get; protected set; }
        public String Folder { get; protected set; }
        public String FolderExtended { get; protected set; }
        public String Name { get; protected set; }

        public Boolean HasConfig { get; private set; }
        public Dictionary<UInt32, String> Maps { get; private set; }
        protected Dictionary<String, String> versions;
        public Dictionary<String, Byte> UserMessages { get; private set; }
        protected StringCollection resourceBlacklist;

        public Game()
        {
        }

        public Game(SteamGameInfo sgi)
        {
            this.Engine = sgi.Engine;
            this.AppId = sgi.AppId;
            this.Folder = sgi.GameFolder;
            this.FolderExtended = sgi.GameFolderExtended;
            this.Name = sgi.GameName;
        }

        protected void ReadConfig()
        {
            String configFileName = Config.ProgramPath + "\\config\\" + (Engine == Engines.HalfLife ? "goldsrc" : "source") + "\\" + Folder + ".json";

            if (!File.Exists(configFileName))
            {
                return;
            }

            GameConfig config;

            using (StreamReader stream = new StreamReader(configFileName))
            {
                Serializer serializer = new Serializer(typeof(GameConfig));
                config = (GameConfig)serializer.Deserialize(stream);
            }

            HasConfig = true;
            versions = new Dictionary<String, String>(config.Versions.Length);

            foreach (GameConfig.Version version in config.Versions)
            {
                versions.Add(version.Checksum, version.Name);
            }

            Maps = new Dictionary<UInt32, String>(config.Maps.Length);

            foreach (GameConfig.Map map in config.Maps)
            {
                Maps.Add(map.Checksum, map.Name);
            }

            UserMessages = new Dictionary<String, Byte>(config.UserMessages.Length);

            foreach (GameConfig.UserMessage userMessage in config.UserMessages)
            {
                UserMessages.Add(userMessage.Name, userMessage.Id);
            }

            resourceBlacklist = new StringCollection();

            foreach (GameConfig.Resource resource in config.ResourceBlacklist)
            {
                resourceBlacklist.Add(resource.Name);
            }
        }

        public String FindVersionName(String checksum)
        {
            if (versions == null)
            {
                return null;
            }

            if (versions.ContainsKey(checksum))
            {
                return (String)versions[checksum];
            }

            return (String)versions["default"];
        }

        public virtual Int32 FindVersion(String checksum)
        {
            return 0;
        }

        public Boolean BuiltInMapExists(UInt32 checksum, String name)
        {
            if (Maps == null)
            {
                return false;
            }

            if (!Maps.ContainsKey(checksum))
            {
                return false;
            }

            if (name == (String)Maps[checksum])
            {
                return true;
            }

            return false;
        }

        public virtual Boolean IsBetaSteamHltvDemo(HalfLifeDemo demo)
        {
            return false;
        }

        public virtual Boolean CanAnalyse(Demo demo)
        {
            return false;
        }

        public virtual Boolean CanConvertNetworkProtocol(Demo demo)
        {
            return false;
        }

        public virtual Boolean CanRemoveFadeToBlack(Demo demo)
        {
            return false;
        }

        public virtual Boolean CanRemoveHltvAds(Demo demo)
        {
            return false;
        }

        public virtual Boolean CanRemoveHltvSlowMotion(Demo demo)
        {
            return false;
        }

        public virtual SolidColorBrush TeamColour(String team)
        {
            return Brushes.Gray;
        }

        public virtual Int32 NewRoundEventId(Demo demo)
        {
            return -1;
        }

        /// <summary>
        /// Handles demo conversion of the svc_resourcelist message.
        /// </summary>
        /// <returns>False if the resource should be removed from the resource list, otherwise false.</returns>
        public virtual Boolean ConvertResourceListCallback(Demo demo, UInt32 type, UInt32 index, ref String name)
        {
            return true;
        }

        public virtual void ConvertEventCallback(Demo demo, HalfLifeDelta delta, UInt32 eventIndex)
        {
        }

        public virtual void ConvertPacketEntititiesCallback(HalfLifeDelta delta, String entityType, Int32 gameVersion)
        {
        }

        public virtual void ConvertDeltaDescriptionCallback(Int32 gameVersion, String deltaStructureName, HalfLifeDelta delta)
        {
        }

        // Counter-Strike and DOD specific.
        public virtual void ConvertClCorpseMessageCallback(Int32 gameVersion, BitBuffer bitBuffer)
        {
        }
    }
}
