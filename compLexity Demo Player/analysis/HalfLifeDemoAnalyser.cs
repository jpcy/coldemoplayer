using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading;
using System.Windows.Media;
using System.IO;
using System.Windows;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace compLexity_Demo_Player
{
    public class HalfLifeDemoAnalyser
    {
        public ObservableCollection<Player> Players
        {
            get
            {
                return players;
            }
        }

        public ObservableCollection<HalfLifeGameState.Round> Rounds
        {
            get
            {
                return gameState.Rounds;
            }
        }

        private HalfLifeDemo demo;
        private IAnalysisWindow analysisWindowInterface;
        private IProgressWindow progressWindowInterface;
        private HalfLifeDemoParser parser;
        private ObservableCollection<Player> players = new ObservableCollection<Player>();
        private HalfLifeGameState gameState;
        private Single currentTimestamp = 0.0f;
        private Hashtable titleTable;
        private Boolean reconnecting = false; // reconnecting after the server changed maps
        private readonly Int32 newRoundEventId;

        public HalfLifeDemoAnalyser(HalfLifeDemo demo, IAnalysisWindow analysisWindowInterface, IProgressWindow progressWindowInterface)
        {
            this.demo = demo;
            this.analysisWindowInterface = analysisWindowInterface;
            this.progressWindowInterface = progressWindowInterface;
            newRoundEventId = GameManager.NewRoundEventId(demo);

            gameState = new HalfLifeGameState();
            NewRound();

            titleTable = new Hashtable();

            // TextMsg
            AddTitle("#Target_Bombed", "Target Succesfully Bombed!");
            AddTitle("#VIP_Escaped", "The VIP has escaped!");
            AddTitle("#VIP_Assassinated", "VIP has been assassinated!");
            AddTitle("#Terrorists_Escaped", "The terrorists have escaped!");
            AddTitle("#CTs_PreventEscape", "The CTs have prevented most of the terrorists from escaping!");
            AddTitle("#Escaping_Terrorists_Neutralized", "Escaping terrorists have all been neutralized!");
            AddTitle("#Bomb_Defused", "The bomb has been defused!");
            AddTitle("#CTs_Win", "Counter-Terrorists Win!");
            AddTitle("#Terrorists_Win", "Terrorists Win!");
            AddTitle("#Round_Draw", "Round Draw!");
            AddTitle("#All_Hostages_Rescued", "All Hostages have been rescued!");
            AddTitle("#Target_Saved", "Target has been saved!");
            AddTitle("#Hostages_Not_Rescued", "Hostages have not been rescued!");
            AddTitle("#Terrorists_Not_Escaped", "Terrorists have not escaped!");
            AddTitle("#VIP_Not_Escaped", "VIP has not escaped!");
            AddTitle("#Terrorist_Escaped", "A terrorist has escaped!");
            AddTitle("#Bomb_Planted", "The bomb has been planted!");
            AddTitle("#Game_will_restart_in", "The game will restart in %s1 %s2");
            AddTitle("#Game_bomb_drop", "%s1 dropped the bomb");
            AddTitle("#Game_bomb_pickup", "%s1 picked up the bomb");
            AddTitle("#Game_connected", "%s1 connected");
            AddTitle("#Game_disconnected", "%s1 has left the game");
            AddTitle("#Game_join_ct", "%s1 is joining the Counter-Terrorist force");
            AddTitle("#Game_join_ct_auto", "%s1 is joining the Counter-Terrorist force (auto)");
            AddTitle("#Game_join_terrorist", "%s1 is joining the Terrorist force");
            AddTitle("#Game_join_terrorist_auto", "%s1 is joining the Terrorist force (auto)");
            AddTitle("#Game_kicked", "Kicked %s1");
            AddTitle("#Game_teammate_attack", "%s1 attacked a teammate");

            // SayText
            AddTitle("#Cstrike_Name_Change", "* %s1 changed name to %s2");
            AddTitle("#Cstrike_Chat_CT", "(Counter-Terrorist) ");
            AddTitle("#Cstrike_Chat_T", "(Terrorist) ");
            AddTitle("#Cstrike_Chat_CT_Dead", "*DEAD*(Counter-Terrorist) ");
            AddTitle("#Cstrike_Chat_T_Dead", "*DEAD*(Terrorist) ");
            AddTitle("#Cstrike_Chat_Spec", "(Spectator) ");
            AddTitle("#Cstrike_Chat_All", "");
            AddTitle("#Cstrike_Chat_AllDead", "*DEAD* ");
            AddTitle("#Cstrike_Chat_AllSpec", "*SPEC* ");
        }

        public void Parse()
        {
            parser = new HalfLifeDemoParser(demo);

            // add svc message handlers
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_print, MessagePrint);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_stufftext, MessageStuffText);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_serverinfo, MessageServerInfo);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_updateuserinfo, MessageUpdateUserInfo);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_pings, MessagePings);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_event_reliable, MessageEventReliable);

            // add user message handlers
            parser.AddUserMessageHandler("SayText", MessageSayText);
            parser.AddUserMessageHandler("TextMsg", MessageTextMsg);
            parser.AddUserMessageHandler("ResetHUD", MessageResetHUD);
            parser.AddUserMessageHandler("DeathMsg", MessageDeathMsg);
            parser.AddUserMessageHandler("ScoreInfo", MessageScoreInfo);
            parser.AddUserMessageHandler("TeamInfo", MessageTeamInfo);
            parser.AddUserMessageHandler("TeamScore", MessageTeamScore);

            parser.Open();
            parser.Seek(HalfLifeDemo.HeaderSizeInBytes); // seek past header

            Int32 percentRead = 0;
            Boolean foundPlaybackSegment = false;

            try
            {
                while (true)
                {
                    HalfLifeDemoParser.FrameHeader frameHeader = parser.ReadFrameHeader();

                    if (frameHeader.Type == 0 || frameHeader.Type == 1)
                    {
                        if (!foundPlaybackSegment && frameHeader.Type == 1)
                        {
                            foundPlaybackSegment = true;
                        }

                        HalfLifeDemoParser.GameDataFrameHeader gameDataFrameHeader = parser.ReadGameDataFrameHeader();
                        currentTimestamp = frameHeader.Timestamp;

                        // length can be 0
                        // e.g. GotFrag Demo 15111 (volcano vs 4k).zip
                        if (gameDataFrameHeader.Length > 0)
                        {
                            Byte[] frameData = parser.Reader.ReadBytes((Int32)gameDataFrameHeader.Length);

                            if (frameData.Length != gameDataFrameHeader.Length)
                            {
                                throw new ApplicationException("Gamedata frame length doesn't match header.");
                            }

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
                                throw new ApplicationException("Message parsing error.\n\n" + parser.ComputeMessageLog(), ex);
                            }
                        }
                    }
                    else if (frameHeader.Type == 5) // end of segment
                    {
                        if (foundPlaybackSegment)
                        {
                            progressWindowInterface.UpdateProgress(100);
                            break;
                        }
                    }
                    else
                    {
                        parser.SkipFrame(frameHeader.Type);
                    }

                    // calculate what percent of the file has been read
                    Int32 oldPercentRead = percentRead;

                    percentRead = (Int32)(parser.Position / (Single)parser.FileLength * 100.0f);

                    if (percentRead != oldPercentRead)
                    {
                        progressWindowInterface.UpdateProgress(percentRead);
                    }
                }
            }
            finally
            {
                parser.Close();
            }
        }

        private void GameLogWriteTimestamp()
        {
            analysisWindowInterface.GameLogWrite(Common.DurationString(currentTimestamp) + ": ", Brushes.Brown);
        }

        private void ParseInfoKeyString(Player player, String s)
        {
            String[] infoKeyTokens = s.Split('\\');

            for (int i = 0; i < infoKeyTokens.Length; i += 2)
            {
                String key = infoKeyTokens[i];
                String value = infoKeyTokens[i + 1];

                // find or create InfoKey object
                InfoKey infoKey = Common.FirstOrDefault<InfoKey>(player.InfoKeys, ik => ik.Key == key);

                if (infoKey == null)
                {
                    infoKey = new InfoKey();
                    infoKey.Key = key;

                    // add new infokey
                    player.InfoKeys.Add(infoKey);
                }
                else
                {
                    // don't create a new value entry if the previous value is exactly the same
                    if (infoKey.NewestValueValue == value)
                    {
                        continue;
                    }
                }

                // create infokey value
                InfoKeyValue infoKeyValue = new InfoKeyValue();
                infoKeyValue.Value = value;
                infoKeyValue.Timestamp = currentTimestamp;

                // add new value to infokey values
                infoKey.Values.Add(infoKeyValue);

                // convert steam id
                if (key == "*sid")
                {
                    AddInfoKey(player, "SteamId", Common.CalculateSteamId(value));
                }
            }
        }

        private void AddInfoKey(Player player, String key, String value)
        {
            InfoKey targetInfoKey = Common.FirstOrDefault<InfoKey>(player.InfoKeys, ik => ik.Key == key);

            if (targetInfoKey == null)
            {
                targetInfoKey = new InfoKey();
                targetInfoKey.Key = key;
                player.InfoKeys.Add(targetInfoKey);
            }

            if (targetInfoKey.NewestValueValue != value)
            {
                InfoKeyValue ikv = new InfoKeyValue();
                ikv.Value = value;
                ikv.Timestamp = currentTimestamp;
                targetInfoKey.Values.Add(ikv);
            }
        }

        private void SetTeamScore(String team, Int32 score)
        {
            gameState.GetCurrentRound().SetTeamScore(team, score);
        }

        private void SetPlayerScore(Byte slot, String playerName, Int32 frags, Int32 deaths)
        {
            gameState.GetCurrentRound().SetPlayerScore(slot, playerName, frags, deaths);
        }

        private void SetPlayerTeam(Byte slot, String playerName, String teamName)
        {
            gameState.GetCurrentRound().SetPlayerTeam(slot, playerName, teamName);
        }

        private void AddPlayerNetworkState(Player player, UInt32 ping, UInt32 loss)
        {
            Player.NetworkState networkState = new Player.NetworkState() { Ping = ping, Loss = loss };
            player.NetworkStates.Add(networkState);
        }

        private void NewRound()
        {
            HalfLifeGameState.Round round = gameState.AddRound(currentTimestamp);

            if (round.Number != 1)
            {
                analysisWindowInterface.GameLogWrite("\n");
            }

            GameLogWriteTimestamp();
            analysisWindowInterface.GameLogWrite(String.Format("Round {0}:\r", round.Number), Brushes.Black, TextDecorations.Underline);
        }

        private Player FindPlayer(Byte slot)
        {
            return FindPlayer(slot, -1);
        }

        private Player FindPlayer(Int32 entityId)
        {
            return FindPlayer((Byte)(255), entityId);
        }

        private Player FindPlayer(Byte slot, Int32 entityId)
        {
            return Common.FirstOrDefault<Player>(players, player => (slot != 255 && slot == player.Slot) || (entityId != -1 && entityId == player.Id));
        }

        /// <summary>
        /// Returns a team colour based on a player's team assignment.
        /// </summary>
        /// <param name="player">The player - can be null.</param>
        /// <returns></returns>
        private SolidColorBrush PlayerGetTeamColour(Player player)
        {
            SolidColorBrush result = null;

            if (player != null)
            {
                result = GameManager.TeamColour(demo, player.Team);

                if (result != null)
                {
                    return result;
                }
            }

            return Brushes.Gray;
        }

        #region Titles
        private void AddTitle(String token, String format)
        {
            titleTable.Add(token, format);
        }

        private String Detokenise(List<String> stringList)
        {
            if (stringList.Count == 0)
            {
                return null;
            }

            // check token
            String token = stringList[0];

            if (!token.StartsWith("#"))
            {
                return null;
            }

            // find format string that applies to token
            String format = (String)titleTable[token];

            if (format == null)
            {
                return null;
            }

            // fill in format string
            String result = format;

            while (true)
            {
                Int32 wildcardIndex = result.IndexOf("%s");

                if (wildcardIndex == -1 || wildcardIndex + 2 >= result.Length || !Char.IsNumber(result, wildcardIndex + 2))
                {
                    break;
                }

                // get number following "%s"
                Int32 wildcardNumber = result[wildcardIndex + 2] - '0';

                // replace the wildcard with the correct string
                result = result.Replace(String.Format("%s{0}", wildcardNumber), stringList[wildcardNumber]);
            }

            return result;
        }
        #endregion

        #region Messages Handlers
        private void MessagePrint()
        {
            // parse "status" command and add steam id's to players' infokeys
            Regex r = new Regex(@"^#\s*(\d*)\s*" + "\"[^\"]*\"" + @"\s*\d*\s*(.*)");
            Match m = r.Match(parser.BitBuffer.ReadString());

            if (m.Success)
            {
                Byte slot = Byte.Parse(m.Groups[1].ToString());
                String steamId = m.Groups[2].ToString().Replace("STAM_", "STEAM_"); // lol valve

                Player player = FindPlayer(slot);

                if (player != null)
                {
                    AddInfoKey(player, "SteamId", steamId);
                }
            }
        }

        private void MessageStuffText()
        {
            String text = parser.BitBuffer.ReadString();

            if (text.Equals("reconnect\n"))
            {
                reconnecting = true;

                // TODO: clear player history etc.
            }
        }

        private void MessageServerInfo()
        {
            parser.Seek(4); // network protocol
            parser.Seek(4); // process count
            parser.Seek(4); // "munged" map checksum
            parser.Seek(16); // client.dll checksum

            parser.Seek(1); // max clients
            parser.Seek(1); // recorder slot

            // skip unknown byte, game folder
            parser.Seek(1);
            parser.BitBuffer.ReadString();

            // see base handler
            if (demo.NetworkProtocol > 43)
            {
                parser.BitBuffer.ReadString(); // server name
            }

            String mapName = parser.BitBuffer.ReadString();

            if (demo.NetworkProtocol == 45)
            {
                Byte extraInfo = parser.BitBuffer.ReadByte();
                parser.Seek(-1);

                if (extraInfo != (Byte)HalfLifeDemoParser.MessageId.svc_sendextrainfo)
                {
                    parser.BitBuffer.ReadString(); // skip mapcycle

                    if (parser.BitBuffer.ReadByte() > 0)
                    {
                        parser.Seek(36);
                    }
                }
            }
            else
            {
                parser.BitBuffer.ReadString(); // skip mapcycle

                if (demo.NetworkProtocol > 43)
                {
                    if (parser.BitBuffer.ReadByte() > 0)
                    {
                        parser.Seek(21);
                    }
                }
            }

            if (reconnecting)
            {
                analysisWindowInterface.GameLogWrite(String.Format("\rServer is changing the map to \"{0}\"\r\r", mapName));
                reconnecting = false;
            }
        }

        private void MessageUpdateUserInfo()
        {
            Byte slot = parser.BitBuffer.ReadByte();
            Int32 entityId = parser.BitBuffer.ReadInt32();

            // note the slot+1, usermessages must use 0 for empty slot or something...
            // updateuserinfo starts at 0
            // user messages start at 1
            slot++;

            // find\create player object
            Player newPlayer = Common.FirstOrDefault<Player>(players, p => p.Id == entityId);

            if (newPlayer == null)
            {
                newPlayer = new Player { Id = entityId };
                players.Add(newPlayer);
            }

            newPlayer.Slot = slot;

            // create infokey string
            String s = parser.BitBuffer.ReadString();

            if (demo.NetworkProtocol > 43)
            {
                parser.Seek(16);
            }

            if (s.Length == 0)
            {
                // 0 length text = a player just left and another player's slot is being changed
                // don't need to do anything, slots are always updated above
                return;
            }

            // parse infokey string
            ParseInfoKeyString(newPlayer, s.Remove(0, 1)); // trim leading slash
        }

        private void MessagePings()
        {
            if (demo.NetworkProtocol <= 43)
            {
                parser.BitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            while (parser.BitBuffer.ReadBoolean())
            {
                UInt32 slot = parser.BitBuffer.ReadUnsignedBits(5) + 1;
                UInt32 ping = parser.BitBuffer.ReadUnsignedBits(12);
                UInt32 loss = parser.BitBuffer.ReadUnsignedBits(7);

                Player player = FindPlayer((Byte)slot);

                if (player != null)
                {
                    AddPlayerNetworkState(player, ping, loss);
                }
            }

            parser.BitBuffer.SkipRemainingBits();
            parser.BitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        private void MessageEventReliable()
        {
            if (demo.NetworkProtocol <= 43)
            {
                parser.BitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            // read message
            UInt32 id = parser.BitBuffer.ReadUnsignedBits(10);
            parser.GetDeltaStructure("event_t").ReadDelta(parser.BitBuffer, null);
            Boolean delayBit = parser.BitBuffer.ReadBoolean();

            if (delayBit)
            {
                parser.BitBuffer.SeekBits(16);
            }

            parser.BitBuffer.SkipRemainingBits();
            parser.BitBuffer.Endian = BitBuffer.EndianType.Little;

            // check if new round has started
            if (id == newRoundEventId)
            {
                NewRound();
            }
        }

        private void MessageSayText()
        {
            // read message
            Int32 length = parser.BitBuffer.ReadByte();
            Byte slot = parser.BitBuffer.ReadByte();
            List<String> stringList = ReadTextStrings(length - 1);

            // write formatted message to log
            String title = Detokenise(stringList);

            // special case for name change
            if ((String)stringList[0] == "#Cstrike_Name_Change")
            {
                GameLogWriteTimestamp();
                analysisWindowInterface.GameLogWrite(title + "\r");
                return;
            }

            Player player = FindPlayer(slot);

            if (title != null)
            {
                GameLogWriteTimestamp();
                analysisWindowInterface.GameLogWrite(title);

                // player name
                if (slot == 0)
                {
                    analysisWindowInterface.GameLogWrite("<" + demo.ServerName + ">", Brushes.Gray);
                }
                else if (player == null || player.Name == null)
                {
                    analysisWindowInterface.GameLogWrite("UNKNOWN", Brushes.Gray);
                }
                else
                {
                    analysisWindowInterface.GameLogWrite(player.Name, PlayerGetTeamColour(player));
                }

                // chat text
                analysisWindowInterface.GameLogWrite(" :  " + (String)stringList[1] + "\r");
            }
            //else if (demo.IsBetaSteam())
            else
            {
                GameLogWriteTimestamp();
                analysisWindowInterface.GameLogWrite((String)stringList[0] + "\r");
            }
        }

        // game connected, game disconnected, bomb drop, bomb pickup etc.
        private void MessageTextMsg()
        {
            // read message
            Int32 length = parser.BitBuffer.ReadByte();
            Byte slot = parser.BitBuffer.ReadByte();
            List<String> stringList = ReadTextStrings(length - 1);

            // write formatted message to log
            String title = Detokenise(stringList);

            if (title != null)
            {
                GameLogWriteTimestamp();
                analysisWindowInterface.GameLogWrite(title + "\r");
            }
        }

        private void MessageResetHUD()
        {
            Int32 length = parser.FindUserMessageLength("ResetHUD");
            parser.Seek(length);

            if (demo.NetworkProtocol <= 43)
            {
                NewRound();
            }
        }

        private void MessageDeathMsg()
        {
            // read message
            Int32 endByteIndex = parser.FindUserMessageLength("DeathMsg") + parser.BitBuffer.CurrentByte;

            Byte killerSlot = parser.BitBuffer.ReadByte();
            Byte victimSlot = parser.BitBuffer.ReadByte();
            Byte headshot = parser.BitBuffer.ReadByte();
            String weaponName = parser.BitBuffer.ReadString();

            parser.BitBuffer.SeekBytes(endByteIndex, SeekOrigin.Begin);

            // write formatted message to log
            Player killer = FindPlayer(killerSlot);
            Player victim = FindPlayer(victimSlot);
            String killerName = ((killer == null || killer.Name == null) ? "UNKNOWN" : killer.Name);
            String victimName = ((victim == null || victim.Name == null) ? "UNKNOWN" : victim.Name);

            GameLogWriteTimestamp();

            if (headshot == 1)
            {
                analysisWindowInterface.GameLogWrite("*** ");
            }

            if (weaponName == "world" || weaponName == "worldspawn")
            {
                analysisWindowInterface.GameLogWrite(victimName, PlayerGetTeamColour(victim));
                analysisWindowInterface.GameLogWrite(" suicided.");
            }
            else if (weaponName == "trigger_hurt")
            {
                analysisWindowInterface.GameLogWrite(victimName, PlayerGetTeamColour(victim));
                analysisWindowInterface.GameLogWrite(" died.");
            }
            else
            {
                analysisWindowInterface.GameLogWrite(killerName, PlayerGetTeamColour(killer));
                analysisWindowInterface.GameLogWrite(" killed ");
                analysisWindowInterface.GameLogWrite(victimName, PlayerGetTeamColour(victim));
                analysisWindowInterface.GameLogWrite(" with ");

                if (headshot == 1)
                {
                    analysisWindowInterface.GameLogWrite("a headshot from ");
                }

                analysisWindowInterface.GameLogWrite(weaponName);
            }

            if (headshot == 1)
            {
                analysisWindowInterface.GameLogWrite(" ***");
            }

            analysisWindowInterface.GameLogWrite("\r");
        }

        private void MessageScoreInfo()
        {
            Int32 endByteIndex = parser.FindUserMessageLength("ScoreInfo") + parser.BitBuffer.CurrentByte;

            Byte slot = parser.BitBuffer.ReadByte();
            Int16 frags = parser.BitBuffer.ReadInt16();
            Int16 deaths = parser.BitBuffer.ReadInt16();
            parser.Seek(2); // class id?
            Int16 teamId = parser.BitBuffer.ReadInt16();

            if (teamId != 1 && teamId != 2) // T, CT (no spectators!)
            {
                return;
            }

            Player player = FindPlayer(slot);

            if (player == null)
            {
                return;
            }

            SetPlayerScore(slot, player.Name, frags, deaths);

            parser.BitBuffer.SeekBytes(endByteIndex, SeekOrigin.Begin);
        }

        private void MessageTeamInfo()
        {
            Int32 endByteIndex = parser.FindUserMessageLength("TeamInfo") + parser.BitBuffer.CurrentByte;

            Byte slot = parser.BitBuffer.ReadByte();
            String team = parser.BitBuffer.ReadString();

            Player player = FindPlayer(slot);

            if (player != null)
            {
                // handle game log
                player.Team = team;

                // handle player score
                SetPlayerTeam(slot, player.Name, team);
            }

            parser.BitBuffer.SeekBytes(endByteIndex, SeekOrigin.Begin);
        }

        private void MessageTeamScore()
        {
            Int32 endByteIndex = parser.FindUserMessageLength("TeamScore") + parser.BitBuffer.CurrentByte;

            if (demo.GameFolderName == "dod")
            {
                Byte teamId = parser.BitBuffer.ReadByte();
                Int16 score = parser.BitBuffer.ReadInt16();
                SetTeamScore(String.Format("{0}", teamId), (Int32)score);
            }
            else
            {
                String name = parser.BitBuffer.ReadString();
                Int16 score = parser.BitBuffer.ReadInt16();
                SetTeamScore(name, (Int32)score);
            }

            parser.BitBuffer.SeekBytes(endByteIndex, SeekOrigin.Begin);
        }

        #endregion

        private List<String> ReadTextStrings(Int32 length) // TODO: rename
        {
            Int32 bytesRead = 0;
            List<String> stringList = new List<String>();

            while (bytesRead < length)
            {
                Int32 currentByte = parser.BitBuffer.CurrentByte;
                String s = parser.BitBuffer.ReadString();
                bytesRead += parser.BitBuffer.CurrentByte - currentByte;

                // remove newlines and 'special' half-life characters from string (SOH etc.)
                List<Int32> removeCharIndexList = new List<Int32>();
                for (Int32 i = 0; i < s.Length; i++)
                {
                    if (s[i] == '\n' || s[i] == 0x01 || s[i] == 0x02 || s[i] == 0x03 || s[i] == 0x04)
                    {
                        removeCharIndexList.Add(i);
                    }
                }

                Int32 charsRemoved = 0; // need to decrement saved indicies as we remove chars
                foreach (Int32 i in removeCharIndexList)
                {
                    s = s.Remove(i - charsRemoved, 1);
                    charsRemoved++;
                }

                // add string to list
                if (s.Length > 0)
                {
                    stringList.Add(s);
                }
            }

            return stringList;
        }

        public class Player : NotifyPropertyChangedItem
        {
            public class NetworkState
            {
                public UInt32 Ping { get; set; }
                public UInt32 Loss { get; set; }
            }

            public String Name
            {
                get
                {
                    InfoKey result = Common.FirstOrDefault<InfoKey>(infoKeys, ik => ik.Key == "name");

                    if (result != null)
                    {
                        return result.NewestValueValue;
                    }

                    return null;
                }
            }

            public Int32 Id { get; set; }
            public Byte Slot { get; set; }
            public ObservableCollection<InfoKey> InfoKeys
            {
                get
                {
                    return infoKeys;
                }
            }

            public String Team { get; set; } // used by game log, not data bound
            public List<NetworkState> NetworkStates
            {
                get
                {
                    return networkStates;
                }
            }

            private ObservableCollection<InfoKey> infoKeys = new ObservableCollection<InfoKey>();
            private List<NetworkState> networkStates = new List<NetworkState>();
        }

        public class InfoKey : NotifyPropertyChangedItem
        {
            public String Key { get; set; }
            public String NewestValueValue
            {
                get
                {
                    if (values.Count > 0)
                    {
                        return values[values.Count - 1].Value;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            public ObservableCollection<InfoKeyValue> Values
            {
                get
                {
                    return values;
                }
            }

            private ObservableCollection<InfoKeyValue> values = new ObservableCollection<InfoKeyValue>();
        }

        public class InfoKeyValue : NotifyPropertyChangedItem
        {
            public String Value { get; set; }
            public Single Timestamp { get; set; }
        }
    }
}
