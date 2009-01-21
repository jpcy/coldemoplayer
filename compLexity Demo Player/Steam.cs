using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace compLexity_Demo_Player
{
    public class SteamGameInfo
    {
        [XmlAttribute("appId")]
        public Int32 AppId;

        [XmlAttribute("folder")]
        public String GameFolder; // e.g. cstrike

        [XmlAttribute("gameFolderExtended")]
        public String GameFolderExtended; // e.g. counter-strike

        [XmlAttribute("name")]
        public String GameName; // e.g. Counter-Strike
    }

    public class SteamGameInfoListing
    {
        public SteamGameInfo[] GoldSrc;
        public SteamGameInfo[] Source;
    }

    public static class Steam
    {
        private static readonly String fileName = "steam.xml";
        private static Hashtable goldSrcTable;
        private static Hashtable sourceTable;

        public static void Initialise(String configPath)
        {
            String configFullPath = configPath + "\\" + fileName;

            if (!File.Exists(configFullPath))
            {
                return;
            }

            SteamGameInfoListing list = new SteamGameInfoListing();

            // deserialize
            list = (SteamGameInfoListing)Common.XmlFileDeserialize(configFullPath, typeof(SteamGameInfoListing));

            // insert list contents into hash tables
            goldSrcTable = new Hashtable();
            sourceTable = new Hashtable();

            HashTableInsertList(goldSrcTable, list.GoldSrc);
            HashTableInsertList(sourceTable, list.Source);
        }

        private static void HashTableInsertList(Hashtable table, SteamGameInfo[] list)
        {
            foreach (SteamGameInfo info in list)
            {
                table.Add(info.GameFolder, info);
            }
        }

        public static SteamGameInfo GetGameInfo(Boolean sourceEngine, String gameFolder)
        {
            if (sourceEngine)
            {
                return (SteamGameInfo)sourceTable[gameFolder];
            }
            else
            {
                return (SteamGameInfo)goldSrcTable[gameFolder];
            }
        }
    }
}
