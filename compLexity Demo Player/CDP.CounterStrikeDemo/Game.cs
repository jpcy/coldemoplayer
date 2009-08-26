using System;
using System.Xml.Serialization;

namespace CDP.CounterStrikeDemo
{
    public class Game : Core.SteamGame
    {
        public class Version
        {
            [XmlAttribute("name")]
            public string Name { get; set; }

            [XmlAttribute("checksum")]
            public string Checksum { get; set; }
        }

        public class Map
        {
            [XmlAttribute("name")]
            public string Name { get; set; }

            [XmlAttribute("checksum")]
            public uint Checksum { get; set; }
        }

        public class UserMessage
        {
            [XmlAttribute("name")]
            public string Name { get; set; }

            [XmlAttribute("id")]
            public byte Id { get; set; }
        }

        public class Resource
        {
            [XmlAttribute("name")]
            public string Name { get; set; }
        }

        public Version[] Versions { get; set; }
        public Map[] Maps { get; set; }
        public UserMessage[] UserMessages { get; set; }
        public Resource[] ResourceBlacklist { get; set; }

        public override string GetVersionName(string checksum)
        {
            return base.GetVersionName(checksum);
        }

        public override int GetVersion(string checksum)
        {
            return base.GetVersion(checksum);
        }
    }
}
