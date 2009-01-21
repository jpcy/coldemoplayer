using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace compLexity_Demo_Player
{
    public class GameConfig
    {
        public class Version
        {
            [XmlAttribute("name")]
            public String Name; // TODO: change to properties?

            [XmlAttribute("checksum")]
            public String Checksum;
        }

        public class Map
        {
            [XmlAttribute("name")]
            public String Name;

            [XmlAttribute("checksum")]
            public UInt32 Checksum;
        }

        public Version[] Versions;
        public Map[] Maps;

        [XmlIgnore]
        public String GameFolder { get; set; }
        private Hashtable versionTable;
        private Hashtable mapTable;

        public void Initialise(String gameFolder)
        {
            GameFolder = gameFolder;

            versionTable = new Hashtable();

            foreach (Version v in Versions)
            {
                versionTable.Add(v.Checksum, v.Name);
            }

            mapTable = new Hashtable();

            foreach (Map m in Maps)
            {
                mapTable.Add(m.Checksum, m.Name);
            }
        }

        public String FindVersion(String checksum)
        {
            String version = (String)versionTable[checksum.ToUpper()];

            if (version == null)
            {
                // see if there's a "default" case
                return (String)versionTable["default"];
            }

            return version;
        }

        public Boolean MapExists(UInt32 checksum, String name)
        {
            String foundMapName = (String)mapTable[checksum];

            if (foundMapName != null)
            {
                if (name == foundMapName)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public static class GameConfigList
    {
        public static Hashtable GoldSrcGameTable; // FIXME
        private static Hashtable sourceGameTable;

        public static void Initialise()
        {
            GoldSrcGameTable = new Hashtable();
            sourceGameTable = new Hashtable();

            EnumerateConfigFolder(GoldSrcGameTable, "goldsrc");
            //EnumerateConfigFolder(sourceGameTable, "source");
        }

        public static GameConfig Find(String gameFolder, String engineName)
        {
            Hashtable table = null;

            if (engineName == "goldsrc")
            {
                table = GoldSrcGameTable;
            }
            else if (engineName == "source")
            {
                table = sourceGameTable;
            }

            return (GameConfig)table[gameFolder];
        }

        private static void EnumerateConfigFolder(Hashtable table, String engineName)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Config.Settings.ProgramPath + "\\config\\" + engineName);

            foreach (FileInfo fi in directoryInfo.GetFiles("*.xml", SearchOption.TopDirectoryOnly))
            {
                String gameFolder = Path.GetFileNameWithoutExtension(fi.Name);
                GameConfig newGameConfig = (GameConfig)Common.XmlFileDeserialize(fi.FullName, typeof(GameConfig));
                newGameConfig.Initialise(gameFolder);
                table.Add(gameFolder, newGameConfig);
            }
        }
    }
}
