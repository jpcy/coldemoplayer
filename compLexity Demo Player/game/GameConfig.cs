using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace compLexity_Demo_Player
{
    public class GameConfig
    {
        public class Version
        {
            [XmlAttribute("name")]
            public String Name { get; set; }

            [XmlAttribute("checksum")]
            public String Checksum { get; set; }
        }

        public class Map
        {
            [XmlAttribute("name")]
            public String Name { get; set; }

            [XmlAttribute("checksum")]
            public UInt32 Checksum { get; set; }
        }

        public Version[] Versions { get; set; }
        public Map[] Maps { get; set; }
    }
}
