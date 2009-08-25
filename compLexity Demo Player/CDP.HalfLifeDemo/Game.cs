using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.HalfLifeDemo
{
    public class Game
    {
        public int SteamAppId { get; protected set; }
        public string SteamAppFolder { get; protected set; }
        public string SteamGameFolder { get; protected set; }
        public string[] GameFolders { get; protected set; }
        public string Name { get; protected set; }

        public Game(int steamAppId, string steamAppFolder, string steamGameFolder, string[] gameFolders, string name)
        {
            SteamAppId = steamAppId;
            SteamAppFolder = steamAppFolder;
            SteamGameFolder = steamGameFolder;
            GameFolders = gameFolders;
            Name = name;
        }

        public virtual string GetVersionName(string checksum)
        {
            return null;
        }

        public virtual int GetVersion(string checksum)
        {
            return 0;
        }
    }
}
