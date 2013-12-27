using System;
using System.Collections.Generic;
using System.Text;

namespace compLexity_Demo_Player
{
    /// <summary>
    /// Represents a single entry deserialized from steam.json.
    /// </summary>
    public class SteamGameInfo
    {
        public Game.Engines Engine { get; set; }
        public Int32 AppId { get; set; }
        public String GameFolder { get; set; } // e.g. cstrike
        public String GameFolderExtended { get; set; } // e.g. counter-strike
        public String GameName { get; set; } // e.g. Counter-Strike
        public Boolean UsesCommonFolder { get; set; } // uses "SteamApps/common" folder instead of "SteamApps/<steam account name>"
        public String ExecutableName { get; set; } // executable name, including the ".exe". e.g. "hl2.exe" or "csgo.exe"
    }
}
