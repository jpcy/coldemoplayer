using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CDP.HalfLife
{
    public class Game
    {
        public int AppId { get; set; }
        public string AppFolder { get; set; }
        public string ModFolder { get; set; }
        public string Name { get; set; }
        public string[] DemoGameFolders { get; set; }

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

        public string GetVersionName(string checksum)
        {
            Version version = Versions.FirstOrDefault(v => v.Checksum == checksum);

            if (version == null)
            {
                // No matching checksum, use the default version.
                version = Versions.FirstOrDefault(v => v.Checksum == "default");
            }

            if (version == null)
            {
                // Game doesn't have version information.
                return null;
            }

            return version.Name;
        }

        public virtual int GetVersion(string checksum)
        {
            return 0;
        }

        public bool BuiltInMapExists(uint checksum, string name)
        {
            return Maps.SingleOrDefault(m => m.Checksum == checksum && m.Name == name) != null;
        }
    }
}
