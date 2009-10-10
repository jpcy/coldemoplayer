using System;
using System.Xml.Serialization;
using System.Linq;
using CDP.Core.Extensions;

namespace CDP.CounterStrike
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
            Version version = Versions.FirstOrDefault(v => v.Checksum == checksum);

            if (version == null)
            {
                return Versions.First(v => v.Checksum == "default").Name;
            }

            return version.Name;
        }

        public override int GetVersion(string checksum)
        {
            Version version = Versions.FirstOrDefault(v => v.Checksum == checksum);

            if (version == null)
            {
                version = Versions.First(v => v.Checksum == "default");
            }

            // Translate the string names defined in the XML config file (e.g. "1.3") to the enumeration Versions (e.g. "CounterStrike13").
            return (int)Enum.Parse(typeof(Demo.Versions), "CounterStrike" + version.Name.RemoveChars('.'));
        }

        public bool BuiltInMapExists(uint checksum, string name)
        {
            return Maps.SingleOrDefault(m => m.Checksum == checksum && m.Name == name) != null;
        }
    }
}
