using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace compLexity_Demo_Player
{
    /// <summary>
    /// Represents a single entry deserialized from steam.xml.
    /// </summary>
    public class SteamGameInfo
    {
        [XmlAttribute("engine")]
        public Game.Engines Engine { get; set; }

        [XmlAttribute("appId")]
        public Int32 AppId { get; set; }

        [XmlAttribute("folder")]
        public String GameFolder { get; set; } // e.g. cstrike

        [XmlAttribute("gameFolderExtended")]
        public String GameFolderExtended { get; set; } // e.g. counter-strike

        [XmlAttribute("name")]
        public String GameName { get; set; } // e.g. Counter-Strike
    }
}
