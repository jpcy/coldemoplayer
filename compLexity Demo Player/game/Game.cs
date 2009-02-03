using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.Xml.Serialization;

namespace compLexity_Demo_Player
{
    public class Game
    {
        public enum Engines
        {
            HalfLife,
            Source
        }

        public Engines Engine { get; private set; }
        public Int32 AppId { get; private set; }
        public String Folder { get; private set; }
        public String FolderExtended { get; private set; }
        public String Name { get; private set; }

        public Boolean HasConfig { get; private set; }
        public Dictionary<UInt32, String> Maps { get; private set; }
        protected Dictionary<String, String> versions;

        public Game(SteamGameInfo sgi)
        {
            this.Engine = sgi.Engine;
            this.AppId = sgi.AppId;
            this.Folder = sgi.GameFolder;
            this.FolderExtended = sgi.GameFolderExtended;
            this.Name = sgi.GameName;
        }

        public void ReadConfig(GameConfig config)
        {
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
        }

        public String FindVersion(String checksum)
        {
            if (versions.ContainsKey(checksum))
            {
                return (String)versions[checksum];
            }

            return (String)versions["default"];            
        }

        public Boolean BuiltInMapExists(UInt32 checksum, String name)
        {
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

        public virtual Boolean CanAnalyse(Demo demo)
        {
            return false;
        }
    }
}
