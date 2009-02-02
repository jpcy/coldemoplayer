using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections; // ArrayList
using System.Threading;

namespace compLexity_Demo_Player
{
    public class HalfLifeDemo : Demo
    {
        public class Player
        {
            public Byte Slot;
            public Int32 Id;
            public InfoKeyList InfoKeys;

            public String Name
            {
                get
                {
                    return InfoKeys.FindNewestValue("name");
                }
            }

            public Player()
            {
                InfoKeys = new InfoKeyList();
            }
        }

        public enum GameVersionEnum
        {
            Unknown,
            CounterStrike10,
            CounterStrike11,
            CounterStrike13,
            CounterStrike14,
            CounterStrike15,
            CounterStrike16
        }

        public enum EngineVersionEnum
        {
            Unknown,
            HalfLife1104,
            HalfLife1106,
            HalfLife1107,
            HalfLife1108,
            HalfLife1109,
            HalfLife1108or1109,
            HalfLife1110,
            HalfLife1111, // Steam
            HalfLife1110or1111
        }

        public const Byte CurrentNetworkProtocol = 48;

        public const Int32 HeaderSizeInBytes = 544;
        public const Int32 DirectoryEntrySizeInBytes = 92;

        private Int64 fileLengthInBytes;
        private Byte recorderSlot;
        private List<Player> playerList;
        private EngineVersionEnum engineVersion = EngineVersionEnum.Unknown;
        private GameVersionEnum gameVersion = GameVersionEnum.Unknown;
        private UInt32 mungedMapChecksum;

        private HalfLifeDemoParser parser;
        private Single currentTimestamp = 0.0f;

        // network protocol conversion
        private Hashtable compatibleUserMessageTable; // CS 1.6 user messages. when converting 1.3-1.5 demos to 1.6, user messages must use id's that are compatible with the mod dll's

        // duplicate loading segments bug
        private Int32 currentFrameIndex;
        private Int32 firstFrameToWriteIndex;

        // "no loading segment" bug
        // GotFrag Demo 16977 (moon vs Catch-Gamer).zip
        private Boolean serverInfoParsed = false;

        #region Properties
        public override String EngineName
        {
            get
            {
                String s = "Half-Life v";
                
                switch (engineVersion)
                {
                    case EngineVersionEnum.HalfLife1104:
                        s += "1.1.0.4";
                        break;

                    case EngineVersionEnum.HalfLife1106:
                        s += "1.1.0.6";
                        break;

                    case EngineVersionEnum.HalfLife1107:
                        s += "1.1.0.7";
                        break;

                    case EngineVersionEnum.HalfLife1108:
                        s += "1.1.0.8";
                        break;

                    case EngineVersionEnum.HalfLife1109:
                        s += "1.1.0.9";
                        break;

                    case EngineVersionEnum.HalfLife1108or1109:
                        s += "1.1.0.8 or v1.1.0.9";
                        break;

                    case EngineVersionEnum.HalfLife1110:
                        s += "1.1.1.0";
                        break;

                    case EngineVersionEnum.HalfLife1111:
                        s += "1.1.1.1";
                        break;

                    case EngineVersionEnum.HalfLife1110or1111:
                        s += "1.1.1.0 or v1.1.1.1";
                        break;

                    default:
                        return "Half-Life Unknown Version";
                }

                return s;
            }
        }

        public List<Player> Players
        {
            get
            {
                return playerList;
            }
        }

        public GameVersionEnum GameVersion
        {
            get
            {
                return gameVersion;
            }
        }

        #endregion

        public HalfLifeDemo(String fileName)
        {
            fileFullPath = fileName;

            playerList = new List<Player>();

            // initialise variables not guaranteed to be initialised by reading the file
            recorderName = "";
            status = StatusEnum.Ok;
            perspective = PerspectiveEnum.Pov;

            // TODO: read this in from an XML file, and store it in a static class
            compatibleUserMessageTable = new Hashtable();
            compatibleUserMessageTable.Add("HudTextArgs", (Byte?)145);
            compatibleUserMessageTable.Add("ShowTimer", (Byte?)144);
            compatibleUserMessageTable.Add("Fog", (Byte?)143);
            compatibleUserMessageTable.Add("Brass", (Byte?)142);
            compatibleUserMessageTable.Add("BotProgress", (Byte?)141);
            compatibleUserMessageTable.Add("Location", (Byte?)140);
            compatibleUserMessageTable.Add("ItemStatus", (Byte?)139);
            compatibleUserMessageTable.Add("BarTime2", (Byte?)138);
            compatibleUserMessageTable.Add("SpecHealth2", (Byte?)137);
            compatibleUserMessageTable.Add("BuyClose", (Byte?)136);
            compatibleUserMessageTable.Add("BotVoice", (Byte?)135);
            compatibleUserMessageTable.Add("Scenario", (Byte?)134);
            compatibleUserMessageTable.Add("TaskTime", (Byte?)133);
            compatibleUserMessageTable.Add("ShadowIdx", (Byte?)132);
            compatibleUserMessageTable.Add("CZCareerHUD", (Byte?)131);
            compatibleUserMessageTable.Add("CZCareer", (Byte?)130);
            compatibleUserMessageTable.Add("ReceiveW", (Byte?)129);
            compatibleUserMessageTable.Add("ADStop", (Byte?)128);
            compatibleUserMessageTable.Add("ForceCam", (Byte?)127);
            compatibleUserMessageTable.Add("SpecHealth", (Byte?)126);
            compatibleUserMessageTable.Add("HLTV", (Byte?)125);
            compatibleUserMessageTable.Add("HostageK", (Byte?)124);
            compatibleUserMessageTable.Add("HostagePos", (Byte?)123);
            compatibleUserMessageTable.Add("ClCorpse", (Byte?)122);
            compatibleUserMessageTable.Add("BombPickup", (Byte?)121);
            compatibleUserMessageTable.Add("BombDrop", (Byte?)120);
            compatibleUserMessageTable.Add("AllowSpec", (Byte?)119);
            compatibleUserMessageTable.Add("TutorClose", (Byte?)118);
            compatibleUserMessageTable.Add("TutorState", (Byte?)117);
            compatibleUserMessageTable.Add("TutorLine", (Byte?)116);
            compatibleUserMessageTable.Add("TutorText", (Byte?)115);
            compatibleUserMessageTable.Add("VGUIMenu", (Byte?)114);
            compatibleUserMessageTable.Add("Spectator", (Byte?)113);
            compatibleUserMessageTable.Add("Radar", (Byte?)112);
            compatibleUserMessageTable.Add("NVGToggle", (Byte?)111);
            compatibleUserMessageTable.Add("Crosshair", (Byte?)110);
            compatibleUserMessageTable.Add("ReloadSound", (Byte?)109);
            compatibleUserMessageTable.Add("BarTime", (Byte?)108);
            compatibleUserMessageTable.Add("StatusIcon", (Byte?)107);
            compatibleUserMessageTable.Add("StatusText", (Byte?)106);
            compatibleUserMessageTable.Add("StatusValue", (Byte?)105);
            compatibleUserMessageTable.Add("BlinkAcct", (Byte?)104);
            compatibleUserMessageTable.Add("ArmorType", (Byte?)103);
            compatibleUserMessageTable.Add("Money", (Byte?)102);
            compatibleUserMessageTable.Add("RoundTime", (Byte?)101);
            compatibleUserMessageTable.Add("SendAudio", (Byte?)100);
            compatibleUserMessageTable.Add("AmmoX", (Byte?)99);
            compatibleUserMessageTable.Add("ScreenFade", (Byte?)98);
            compatibleUserMessageTable.Add("ScreenShake", (Byte?)97);
            compatibleUserMessageTable.Add("ShowMenu", (Byte?)96);
            compatibleUserMessageTable.Add("SetFOV", (Byte?)95);
            compatibleUserMessageTable.Add("HideWeapon", (Byte?)94);
            compatibleUserMessageTable.Add("ItemPickup", (Byte?)93);
            compatibleUserMessageTable.Add("WeapPickup", (Byte?)92);
            compatibleUserMessageTable.Add("AmmoPickup", (Byte?)91);
            compatibleUserMessageTable.Add("ServerName", (Byte?)90);
            compatibleUserMessageTable.Add("MOTD", (Byte?)89);
            compatibleUserMessageTable.Add("GameMode", (Byte?)88);
            compatibleUserMessageTable.Add("TeamScore", (Byte?)87);
            compatibleUserMessageTable.Add("TeamInfo", (Byte?)86);
            compatibleUserMessageTable.Add("ScoreInfo", (Byte?)85);
            compatibleUserMessageTable.Add("ScoreAttrib", (Byte?)84);
            compatibleUserMessageTable.Add("DeathMsg", (Byte?)83);
            compatibleUserMessageTable.Add("GameTitle", (Byte?)82);
            compatibleUserMessageTable.Add("ViewMode", (Byte?)81);
            compatibleUserMessageTable.Add("InitHUD", (Byte?)80);
            compatibleUserMessageTable.Add("ResetHUD", (Byte?)79);
            compatibleUserMessageTable.Add("WeaponList", (Byte?)78);
            compatibleUserMessageTable.Add("TextMsg", (Byte?)77);
            compatibleUserMessageTable.Add("SayText", (Byte?)76);
            compatibleUserMessageTable.Add("HudText", (Byte?)75);
            compatibleUserMessageTable.Add("HudTextPro", (Byte?)74);
            compatibleUserMessageTable.Add("Train", (Byte?)73);
            compatibleUserMessageTable.Add("Battery", (Byte?)72);
            compatibleUserMessageTable.Add("Damage", (Byte?)71);
            compatibleUserMessageTable.Add("Health", (Byte?)70);
            compatibleUserMessageTable.Add("FlashBat", (Byte?)69);
            compatibleUserMessageTable.Add("Flashlight", (Byte?)68);
            compatibleUserMessageTable.Add("Geiger", (Byte?)67);
            compatibleUserMessageTable.Add("CurWeapon", (Byte?)66);
            compatibleUserMessageTable.Add("ReqState", (Byte?)65);
            compatibleUserMessageTable.Add("VoiceMask", (Byte?)64);
        }

        #region Misc
        /// <summary>
        /// Returns true if the demo should be converted to the current network protocol during writing.
        /// </summary>
        /// <returns></returns>
        public Boolean ConvertNetworkProtocol()
        {
            if (GameFolderName != "cstrike")
            {
                return false;
            }

            if (IsBetaSteam())
            {
                // Return true, although most messages shouldn't be converted - just the network protocol numbers in the header, svc_serverinfo and user message id's (since they're different). Messages should call IsBetaSteam and handle this issue themselves.
                return true;
            }

            if (NetworkProtocol >= 43 && NetworkProtocol <= 46 && Config.Settings.PlaybackConvertNetworkProtocol)
            {
                return true;
            }

            return false;
        }

        public Boolean ConvertToCurrentNetworkProtocol()
        {
            if (ConvertNetworkProtocol())
            {
                return true;
            }

            // FIXME: does the update only apply to these games or all hl1 engine games?
            /*if (NetworkProtocol == 47 && (GameFolderName == "cstrike" || GameFolderName == "czero" || GameFolderName == "czerodeleted" || GameFolderName == "tfc" || GameFolderName == "dmc" || GameFolderName == "ricochet" || GameFolderName == "valve" || GameFolderName == "dod"))
            {
                return true;
            }*/

            if (NetworkProtocol == 47)
            {
                return true;
            }

            return false;
        }

        public Boolean IsBetaSteam()
        {
            if (NetworkProtocol != 46)
            {
                // workaround for johnny r pov on inferno against ocrana
                // newer protocol than usual beta demos but same messed up usermessage indicies...
                if (NetworkProtocol == 47 && Perspective == PerspectiveEnum.Pov && BuildNumber <= 2573)
                {
                    return true;
                }

                return false;
            }

            // may be beta Steam HLTV
            // can only determine this via client dll checksum (i.e. this is mod specific)
            if (GameFolderName == "cstrike")
            {
                return (GameVersion == GameVersionEnum.CounterStrike16);
            }

            // all other mods: assume it's not beta
            return false;
        }
        #endregion

        #region Reading
        protected override void ReadingThread()
        {
            try
            {
                ReadingThreadWorker();
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                mainWindowInterface.Error("Error reading demo file \"" + FileFullPath + "\"", ex);
                demoListViewInterface.DemoLoadingFinished(null);
                return;
            }

            demoListViewInterface.DemoLoadingFinished(this);
        }

        private void ReadingThreadWorker()
        {
            FileStream fs = null;
            BinaryReader br = null;

            // read header
            try
            {
                fs = File.OpenRead(FileFullPath);
                br = new BinaryReader(fs);

                fileLengthInBytes = fs.Length;

                if (fileLengthInBytes < HeaderSizeInBytes)
                {
                    throw new ApplicationException("File length is too short to parse the header.");
                }

                ReadHeader(br);
            }
            finally
            {
                if (br != null)
                {
                    br.Close();
                }

                if (fs != null)
                {
                    fs.Close();
                }
            }

            // create parser
            parser = new HalfLifeDemoParser(this);

            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_setview, ReadMessageSetView);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_print, ReadMessagePrint);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_serverinfo, ReadMessageServerInfo);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_updateuserinfo, ReadMessageUpdateUserInfo);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_hltv, ReadMessageHltv);

            parser.Open();
            parser.Seek(HeaderSizeInBytes); // seek past header
                
            try
            {
                // read and parse frames until the end of the loading segment
                while (true)
                {
                    HalfLifeDemoParser.FrameHeader frameHeader = parser.ReadFrameHeader();

                    // "no loading segment" bug
                    if (frameHeader.Type == 1)
                    {
                        if (serverInfoParsed)
                        {
                            break;
                        }
                    }

                    if (frameHeader.Type == 0 || frameHeader.Type == 1)
                    {
                        HalfLifeDemoParser.GameDataFrameHeader gameDataFrameHeader = parser.ReadGameDataFrameHeader();

                        currentTimestamp = frameHeader.Timestamp;

                        if (gameDataFrameHeader.Length > 0)
                        {
                            Byte[] frameData = parser.Reader.ReadBytes((Int32)gameDataFrameHeader.Length);

                            try
                            {
                                parser.ParseGameDataMessages(frameData);
                            }
                            catch (ThreadAbortException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                throw new ApplicationException("Error parsing gamedata frame.\n\n" + parser.ComputeMessageLog(), ex);
                            }
                        }
                    }
                    else
                    {
                        parser.SkipFrame(frameHeader.Type);
                    }
                }
            }
            finally
            {
                parser.Close();
            }

            // get demo recorder's name
            if (perspective == PerspectiveEnum.Pov)
            {
                foreach (Player p in playerList)
                {
                    if (p.Slot == recorderSlot)
                    {
                        recorderName = p.InfoKeys.FindNewestValue("name");
                    }
                }
            }
        }

        private void ReadHeader(BinaryReader br)
        {
            br.BaseStream.Seek(8, SeekOrigin.Current); // skip magic, DemoFactory checks it
            
            demoProtocol = br.ReadUInt32();

            if (demoProtocol != 5)
            {
                throw new ApplicationException(String.Format("Unknown demo protocol \"{0}\", should be 5.", demoProtocol));
            }

            networkProtocol = br.ReadUInt32();

            if (networkProtocol < 43) // don't support demos older than HL 1.1.0.4/CS 1.0
            {
                throw new ApplicationException(String.Format("Unsupported network protcol \"{0}\", only 43 and higher are supported."));
            }

            mapName = Common.ReadNullTerminatedString(br, 260).ToLower();
            gameFolderName = Common.ReadNullTerminatedString(br, 260).ToLower();
            mapChecksum = br.ReadUInt32();
            Int64 directoryEntriesOffset = br.ReadUInt32();
       
            // check directory entries

            // check offset, should be exactly file length - no. of dir entries (int) + dir entry size * 2
            // otherwise, assume they are corrupt
            if (directoryEntriesOffset != fileLengthInBytes - 4 - (DirectoryEntrySizeInBytes * 2))
            {
                status = StatusEnum.CorruptDirEntries;
                return;
            }

            // seek to directory entries offset
            Int64 newPosition = br.BaseStream.Seek(directoryEntriesOffset, SeekOrigin.Begin); // CHECK ME: is this correct or is an exception thrown if we seek to far?

            if (newPosition != directoryEntriesOffset)
            {
                status = StatusEnum.CorruptDirEntries;
                return;
            }
            
            // read no. of directory entries
            Int32 nDirectoryEntries = br.ReadInt32();

            if (nDirectoryEntries != 2)
            {
                status = StatusEnum.CorruptDirEntries;
                return;
            }

            // read directory entries
            for (Int32 i = 0; i < nDirectoryEntries; i++)
            {
                br.BaseStream.Seek(4, SeekOrigin.Current); // skip number
                String dirEntryTitle = Common.ReadNullTerminatedString(br, 64);
                br.BaseStream.Seek(8, SeekOrigin.Current); // skip flags, cdtrack
                Single dirEntryTime = br.ReadSingle();
                br.BaseStream.Seek(12, SeekOrigin.Current); // skip frames, offset and length (we calculate these ourselves, so corrupt directory entries or not, the demo is treated the same)

                if (dirEntryTitle.ToLower() == "playback")
                {
                    // store demo duration
                    durationInSeconds = Math.Abs(dirEntryTime);
                }
            }
        }

        /// <summary>
        /// Calculates the engine version and type based on the network protocol and build numbers. CalculateGameVersion sould be called before this becaues of issues with HLTV demos and beta Steam demos.
        /// </summary>
        private void CalculateEngineVersionAndType()
        {
            engineVersion = EngineVersionEnum.Unknown;
            engineType = EngineEnum.HalfLife;

            if (networkProtocol == 43)
            {
                if (buildNumber >= 1712)
                {
                    engineVersion = EngineVersionEnum.HalfLife1107;
                }
                else if (buildNumber >= 1600)
                {
                    engineVersion = EngineVersionEnum.HalfLife1106;
                }
                else if (buildNumber >= 1460)
                {
                    engineVersion = EngineVersionEnum.HalfLife1104;
                }
            }
            else if (networkProtocol == 45)
            {
                if (Perspective == PerspectiveEnum.Hltv)
                {
                    if (GameVersion == GameVersionEnum.CounterStrike13)
                    {
                        engineVersion = EngineVersionEnum.HalfLife1108;
                    }
                    else if (GameVersion == GameVersionEnum.CounterStrike14)
                    {
                        engineVersion = EngineVersionEnum.HalfLife1109;
                    }
                    else
                    {
                        engineVersion = EngineVersionEnum.HalfLife1108or1109;
                    }
                }
                else if (buildNumber >= 2006)
                {
                    engineVersion = EngineVersionEnum.HalfLife1109;
                }
                else
                {
                    engineVersion = EngineVersionEnum.HalfLife1108;
                }
            }
            else if (networkProtocol == 46)
            {
                if (IsBetaSteam()) // fucking Valve...
                {
                    engineVersion = EngineVersionEnum.HalfLife1111;
                    engineType = EngineEnum.HalfLifeSteam;
                }
                else if (Perspective == PerspectiveEnum.Hltv)
                {
                    if (GameVersion == GameVersionEnum.CounterStrike15)
                    {
                        engineVersion = EngineVersionEnum.HalfLife1110;
                    }
                    else if (GameVersion == GameVersionEnum.CounterStrike16)
                    {
                        engineVersion = EngineVersionEnum.HalfLife1111;
                    }
                    else
                    {
                        engineVersion = EngineVersionEnum.HalfLife1110or1111;
                    }
                }
                else
                {
                    engineVersion = EngineVersionEnum.HalfLife1110;
                }
            }
            else if (networkProtocol >= 47)
            {
                engineVersion = EngineVersionEnum.HalfLife1111;
                engineType = EngineEnum.HalfLifeSteam;
            }
        }

        /// <summary>
        /// Calculates the game version string based on the client dll checksum.
        /// </summary>
        /// <remarks>Currently only Counter-Strike versions are calculated.</remarks>
        private void CalculateGameVersion()
        {
            if (GameFolderName != "cstrike")
            {
                return;
            }

            GameConfig gameConfig = GameConfigList.Find(GameFolderName, "goldsrc");

            if (gameConfig == null)
            {
                return;
            }

            String versionName = gameConfig.FindVersion(clientDllChecksum);

            // FIXME: this is pretty awful
            if (versionName == "1.0")
            {
                gameVersion = GameVersionEnum.CounterStrike10;
            }
            else if (versionName == "1.1")
            {
                gameVersion = GameVersionEnum.CounterStrike11;
            }
            else if (versionName == "1.3")
            {
                gameVersion = GameVersionEnum.CounterStrike13;
            }
            else if (versionName == "1.4")
            {
                gameVersion = GameVersionEnum.CounterStrike14;
            }
            else if (versionName == "1.5")
            {
                gameVersion = GameVersionEnum.CounterStrike15;
            }
            else
            {
                gameVersion = GameVersionEnum.CounterStrike16;
            }
        }
        #endregion

        #region Writing

        /// <summary>
        /// Writes the demo to the destination folder while performing modifications such as removing the scoreboard or fade to black, possibly converting messages to the current network protocol, as well as re-writing directory entries.
        /// </summary>
        /// <param name="_destinationPath">The destination folder.</param>
        protected override void WritingThread(object _destinationFileName)
        {
            firstFrameToWriteIndex = 0;

            try
            {
                /*
                 * Converted demos: pre-process the loading segment and get the frame index of the last 
                 * svc_serverinfo message in the loading segment.
                 * 
                 * This fixes several bugs:
                 *      1. long (for Half-Life) loading times, since the resources of several maps may be
                 *      loaded.
                 *      
                 *      2. wrong map in resource list
                 *      
                 *      3. random SendAudio CTD (indirectly)
                 */
                if (ConvertNetworkProtocol() && !IsBetaSteam())
                {
                    currentFrameIndex = 0;

                    // initialise parser
                    parser = new HalfLifeDemoParser(this);
                    parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_serverinfo, PreWriteMessageServerInfo);
                    parser.Open();

                    try
                    {
                        parser.Seek(HeaderSizeInBytes);

                        while (true)
                        {
                            HalfLifeDemoParser.FrameHeader frameHeader = parser.ReadFrameHeader();

                            if (frameHeader.Type == 1)
                            {
                                break;
                            }

                            if (frameHeader.Type == 0)
                            {
                                HalfLifeDemoParser.GameDataFrameHeader gameDataFrameHeader = parser.ReadGameDataFrameHeader();
                                Byte[] frameData = parser.Reader.ReadBytes((Int32)gameDataFrameHeader.Length);
                                parser.ParseGameDataMessages(frameData);
                            }
                            else if (frameHeader.Type != 5)
                            {
                                parser.SkipFrame(frameHeader.Type);
                            }

                            currentFrameIndex++;
                        }
                    }
                    finally
                    {
                        parser.Close();
                    }
                }

                // demo writer
                HalfLifeDemoConverter demoConverter = new HalfLifeDemoConverter(this);
                HalfLifeDemoWriter demoWriter = new HalfLifeDemoWriter(this, (IHalfLifeDemoWriter)demoConverter, writeProgressWindowInterface, firstFrameToWriteIndex);

                demoWriter.ThreadWorker((String)_destinationFileName);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (HalfLifeDemoWriter.AbortWritingException)
            {
                writeProgressWindowInterface.CloseWithResult(false);
                return;
            }
            catch (Exception ex)
            {
                writeProgressWindowInterface.Error("Error writing demo file \"" + fileFullPath + "\".", ex, false, null);
                writeProgressWindowInterface.CloseWithResult(false);
                return;
            }

            writeProgressWindowInterface.CloseWithResult(true);
        }

        private Byte UserMessageCallback(Byte messageId)
        {
            String name = parser.FindMessageIdString(messageId);

            Byte? newMessageId = (Byte?)compatibleUserMessageTable[name];

            if (newMessageId == null)
            {
                // shouldn't happen, must be a bad message
                // let the parser handle it
                newMessageId = messageId;
            }

            return (Byte)newMessageId;
        }
        #endregion

        #region Read Message Handlers
        private void ReadMessageSetView()
        {
            UInt16 edict = parser.BitBuffer.ReadUInt16();

            if (mapChecksum == 0)
            {
                mapChecksum = Common.UnMunge3(mungedMapChecksum, (Int32)edict - 1);
            }
        }

        private void ReadMessagePrint()
        {
            String s = parser.BitBuffer.ReadString();

            // get build number
            // FIXME: use regex?
            if (perspective == PerspectiveEnum.Pov && s.Contains("BUILD"))
            {
                s = s.Remove(0, 2);
                s = s.Replace("BUILD ", "");
                s = s.Remove(s.IndexOf(' '));

                buildNumber = Convert.ToInt32(s);
            }
        }

        private void ReadMessageServerInfo()
        {
            parser.Seek(4); // network protocol
            parser.Seek(4); // process count
            mungedMapChecksum = parser.BitBuffer.ReadUInt32();

            // read client.dll checksum
            Byte[] checksum = parser.BitBuffer.ReadBytes(16);

            // convert client.dll checksum to string
            StringBuilder sb = new StringBuilder();

            for (Int32 i = 0; i < checksum.Length; i++)
            {
                sb.Append(checksum[i].ToString("X2"));
            }

            clientDllChecksum = sb.ToString();

            // determine engine and game versions
            CalculateGameVersion();
            CalculateEngineVersionAndType();

            maxClients = parser.BitBuffer.ReadByte();
            recorderSlot = parser.BitBuffer.ReadByte();

            // skip unknown byte, game folder
            parser.Seek(1);
            parser.BitBuffer.ReadString();

            // see base handler
            if (networkProtocol > 43)
            {
                serverName = parser.BitBuffer.ReadString();
            }
            else
            {
                serverName = "Unknown";
            }

            // skip map
            parser.BitBuffer.ReadString();

            if (NetworkProtocol != 45)
            {
                parser.BitBuffer.ReadString(); // skip mapcycle

                if (NetworkProtocol > 43)
                {
                    if (parser.BitBuffer.ReadByte() > 0)
                    {
                        parser.Seek(21);
                    }
                }
            }

            // "no loading segment" bug
            serverInfoParsed = true;
        }

        private void ReadMessageUpdateUserInfo()
        {
            Byte slot = parser.BitBuffer.ReadByte();
            Int32 id = parser.BitBuffer.ReadInt32();
            String s = parser.BitBuffer.ReadString();

            if (NetworkProtocol > 43)
            {
                parser.Seek(16);
            }

            if (s.Length == 0)
            {
                // 0 length text = a player just left and another player's slot is being changed
                // TODO: ?
                return;
            }

            Player player = null;

            // see if player with matching id exists
            foreach (Player p in playerList)
            {
                if (p.Id == id)
                {
                    player = p;
                    break;
                }
            }
              
            // create player if it doesn't exist
            if (player == null)
            {
                player = new Player();
                player.Id = id;
                player.Slot = slot;

                playerList.Add(player);
            }

            if (s.Length == 0)
            {
                // 0 length text = a player just left and another player's slot is being changed
                player.Slot = slot;
                return;
            }

            // parse infokey string
            s = s.Remove(0, 1); // trim leading slash
            string[] infoKeyTokens = s.Split('\\');

            for (Int32 i = 0; i < infoKeyTokens.Length; i += 2)
            {
                player.InfoKeys.Add(infoKeyTokens[i], infoKeyTokens[i + 1], currentTimestamp);
            }
        }

        private void ReadMessageHltv()
        {
            perspective = PerspectiveEnum.Hltv;

            /*
            #define HLTV_ACTIVE				0	// tells client that he's an spectator and will get director commands
            #define HLTV_STATUS				1	// send status infos about proxy 
            #define HLTV_LISTEN				2	// tell client to listen to a multicast stream
             */

            Byte subCommand = parser.BitBuffer.ReadByte();

            if (subCommand == 2) // HLTV_LISTEN
            {
                parser.Seek(8);
            }
            else if (subCommand == 1)
            {
                // TODO: fix this
            }
        }
        #endregion

        #region Pre-Write Message Handlers
        private void PreWriteMessageServerInfo()
        {
            firstFrameToWriteIndex = currentFrameIndex;

            // skip the rest

            parser.Seek(4); // network protocol
            parser.Seek(4); // process count
            parser.Seek(4); // munged map checksum
            parser.Seek(16); // client.dll checksum
            parser.Seek(1); // max clients
            parser.Seek(1); // recorder slot
            parser.Seek(1); // unknown byte
            parser.BitBuffer.ReadString(); // game folder

            // server name
            if (NetworkProtocol > 43)
            {
                parser.BitBuffer.ReadString();
            }

            // skip map
            parser.BitBuffer.ReadString();

            if (NetworkProtocol != 45)
            {
                parser.BitBuffer.ReadString(); // skip mapcycle

                if (NetworkProtocol > 43)
                {
                    if (parser.BitBuffer.ReadByte() > 0)
                    {
                        parser.Seek(21);
                    }
                }
            }
        }
        #endregion
    }
}
