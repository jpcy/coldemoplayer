using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CDP.Core
{
    public class SteamGame
    {
        [XmlAttribute("appId")]
        public int AppId { get; set; }

        [XmlAttribute("appFolder")]
        public string AppFolder { get; set; }

        [XmlAttribute("modFolder")]
        public string ModFolder { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        public string[] DemoGameFolders { get; set; }

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
