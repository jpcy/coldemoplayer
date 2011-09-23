using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Windows.Media;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using ZedGraph;

namespace compLexity_Demo_Player
{
    public class SourceAnalysisWindow : AnalysisWindow
    {
        private class Player
        {
            public String Name;
            public String SteamId;
        }

        private SourceDemoParser parser;
        private Int32 currentRound = 1;
        private UInt32 lastTick = 0;
        private Single currentTimestamp = 0.0f;
        private Int32 currentStringTableIndex = 0;
        private Int32 userInfoStringTableIndex = -1;
        private Int32 userInfoStringTableEntryIndexBits = -1;
        private Hashtable playerTable; // player ID is the hash (UInt32)
        private Hashtable titleTable;

        public SourceAnalysisWindow(Demo demo)
        {
            this.demo = demo;

            playerTable = new Hashtable();
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

            NewRound();

            uiScoreboardTabItem.Visibility = Visibility.Collapsed;
            uiPlayersTabItem.Visibility = Visibility.Collapsed;
            uiNetworkGraphTabItem.Visibility = Visibility.Collapsed;
        }

        protected override void Parse()
        {
            try
            {
                GenerateThread();
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                String errorMessage = "Error generating statisitics.";

                if (((SourceDemo)demo).UnsupportedNetworkProtocol)
                {
                    errorMessage += String.Format("\n\nProbable cause for error: demo uses unsupported network protocol \"{0}\".", demo.NetworkProtocol);
                }

                progressWindowInterface.Error(errorMessage, ex, false, null);
            }

            ParsingFinished(null, null, null);
        }

        private void GenerateThread()
        {
            parser = new SourceDemoParser((SourceDemo)demo);

            parser.AddMessageHandler((Byte)SourceDemoParser.MessageId.SVC_CreateStringTable, MessageCreateStringTable);
            parser.AddMessageHandler((Byte)SourceDemoParser.MessageId.SVC_UpdateStringTable, MessageUpdateStringTable);
            parser.AddMessageHandler((Byte)SourceDemoParser.MessageId.SVC_UserMessage, MessageUserMessage);
            parser.AddMessageHandler((Byte)SourceDemoParser.MessageId.SVC_GameEventList, MessageGameEventList);

            parser.Open();
            parser.Seek(SourceDemo.HeaderSizeInBytes);

            // so we can detect when signon frames stop and packet frames start
            SourceDemoParser.FrameType lastFrameType = SourceDemoParser.FrameType.Signon;

            Int32 percentRead = 0;

            try
            {
                while (true)
                {
                    SourceDemoParser.FrameType frameType = parser.ReadFrameHeader().Type;

                    if (frameType == SourceDemoParser.FrameType.Stop)
                    {
                        progressWindowInterface.UpdateProgress(100);
                        break;
                    }
                    else if (frameType == SourceDemoParser.FrameType.Signon || frameType == SourceDemoParser.FrameType.Packet)
                    {
                        if (lastFrameType == SourceDemoParser.FrameType.Signon && frameType == SourceDemoParser.FrameType.Packet)
                        {
                            parser.AddMessageHandler((Byte)SourceDemoParser.MessageId.NET_Tick, MessageNetTick);
                        }

                        lastFrameType = frameType;

                        parser.ReadCommandInfo();
                        parser.ReadSequenceInfo();

                        Int32 frameLength = parser.Reader.ReadInt32();

                        try
                        {
                            parser.ParsePacketMessages(frameLength);
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
                    else
                    {
                        parser.ParseFrame(frameType);
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
            GameLogWrite(Common.DurationString(currentTimestamp) + ": ", Brushes.Brown);
        }

        private void AddPlayer(UInt32 id, String name, String steamId)
        {
            Player p = (Player)playerTable[id];

            if (p == null)
            {
                p = new Player();
                playerTable.Add(id, p);
            }

            p.Name = name;
            p.SteamId = steamId;
        }

        private String FindPlayerName(UInt32 id)
        {
            String name = String.Format("UNKNOWN ({0})", id);

            Player p = (Player)playerTable[id];

            if (p != null)
            {
                name = p.Name;
            }

            return name;
        }

        private void ParseUserInfo()
        {
            if (demo.NetworkProtocol >= 14)
            {
                parser.BitBuffer.SeekBits(2);
            }

            String playerName = parser.BitBuffer.ReadString(32);
            UInt32 playerId = parser.BitBuffer.ReadUnsignedBits(32);
            String playerSteamId = parser.BitBuffer.ReadString(33);

            AddPlayer(playerId, playerName, playerSteamId);
        }

        private void NewRound()
        {
            if (currentRound != 1)
            {
                GameLogWrite("\r");
            }

            GameLogWriteTimestamp();
            GameLogWrite(String.Format("Round {0}:\r", currentRound), Brushes.Black, TextDecorations.Underline);
            currentRound++;
        }

        #region Titles
        private void AddTitle(String token, String format)
        {
            titleTable.Add(token, format);
        }

        private String Detokenise(String token)
        {
            // check token
            if (!token.StartsWith("#"))
            {
                return null;
            }

            // find format string that applies to token
            return (String)titleTable[token];
        }
        #endregion

        #region Engine Messages
        private void MessageNetTick()
        {
            UInt32 tick = parser.BitBuffer.ReadUInt32();

            if (demo.NetworkProtocol >= 14)
            {
                parser.BitBuffer.SeekBits(32);
            }

            if (lastTick == 0) // first tick message?
            {
                currentTimestamp = 0.0f;
            }
            else
            {
                currentTimestamp += (tick - lastTick) * ((SourceDemo)demo).TimeDeltaPerTick;
            }

            lastTick = tick;
        }

        private void MessageCreateStringTable()
        {
            String tableName = parser.BitBuffer.ReadString();
            Int32 maxEntries = (Int32)parser.BitBuffer.ReadUnsignedBits(16); // TODO: sanity check on maxEntries?
            Int32 entriesBits = Common.LogBase2(maxEntries) + 1;
            Int32 nEntries = (Int32)parser.BitBuffer.ReadUnsignedBits(entriesBits);
            Int32 nBits = (Int32)parser.BitBuffer.ReadUnsignedBits(20);

            UInt32 userDataSize = 0;

            if (parser.BitBuffer.ReadBoolean()) // userdata bit
            {
                userDataSize = parser.BitBuffer.ReadUnsignedBits(12);
                parser.BitBuffer.SeekBits(4); // "user data bits"
            }

            if (tableName == "userinfo")
            {
                userInfoStringTableIndex = currentStringTableIndex;
                userInfoStringTableEntryIndexBits = entriesBits;

                for (int i = 0; i < nEntries; i++)
                {
                    parser.BitBuffer.SeekBits(2); // unknown

                    if (parser.BitBuffer.ReadBoolean()) // delta bit
                    {
                        parser.BitBuffer.SeekBits(10); // 5 bits history index, 5 bits history length
                        parser.BitBuffer.ReadString(); // delta entry
                    }
                    else
                    {
                        parser.BitBuffer.ReadString(); // entry
                    }

                    if (userDataSize == 0) // ??? could be wrong
                    {
                        if (parser.BitBuffer.ReadBoolean()) // userdata bit
                        {
                            // parse userinfo
                            UInt32 nUserDataBytes = parser.BitBuffer.ReadUnsignedBits(12);

                            if (nUserDataBytes > 0)
                            {
                                Int32 currentOffset = parser.BitBuffer.CurrentBit;
                                ParseUserInfo();
                                parser.BitBuffer.SeekBits(currentOffset + (Int32)nUserDataBytes * 8, SeekOrigin.Begin);
                            }

                            if (demo.NetworkProtocol >= 14)
                            {
                                parser.BitBuffer.SeekBits(2); // unknown
                            }
                        }
                    }
                    else
                    {
                        parser.BitBuffer.SeekBits(3); // unknown
                    }
                }
            }
            else
            {
                parser.BitBuffer.SeekBits(nBits);
            }

            currentStringTableIndex++;
        }

        private void MessageUpdateStringTable()
        {
            UInt32 tableId;

            if (demo.DemoProtocol <= 2)
            {
                tableId = parser.BitBuffer.ReadUnsignedBits(4);
            }
            else
            {
                tableId = parser.BitBuffer.ReadUnsignedBits(5);
            }

            UInt32 nEntries = 1;

            if (parser.BitBuffer.ReadBoolean()) // nEntries bit
            {
                nEntries = parser.BitBuffer.ReadUnsignedBits(16);
            }

            UInt32 nBits;

            if (demo.NetworkProtocol >= 14)
            {
                nBits = parser.BitBuffer.ReadUnsignedBits(20);
            }
            else
            {
                nBits = parser.BitBuffer.ReadUnsignedBits(16);
            }

            if (tableId == userInfoStringTableIndex)
            {
                if (userInfoStringTableEntryIndexBits == -1)
                {
                    throw new ApplicationException("SVC_UpdateStringTable without SVC_CreateStringTable first (userinfo).");
                }

                for (int i = 0; i < nEntries; i++)
                {
                    if (!parser.BitBuffer.ReadBoolean()) // relative index bit
                    {
                        parser.BitBuffer.SeekBits(userInfoStringTableEntryIndexBits - 1);
                    }

                    if (parser.BitBuffer.ReadBoolean()) // name bit
                    {
                        parser.BitBuffer.SeekBits(1); // unknown
                        parser.BitBuffer.ReadString();
                    }

                    if (parser.BitBuffer.ReadBoolean()) // userdata bit
                    {
                        UInt32 nUserDataBytes = parser.BitBuffer.ReadUnsignedBits(12);

                        if (nUserDataBytes > 0)
                        {
                            Int32 currentOffset = parser.BitBuffer.CurrentBit;
                            ParseUserInfo();
                            parser.BitBuffer.SeekBits(currentOffset + (Int32)nUserDataBytes * 8, SeekOrigin.Begin);
                        }
                    }
                }
            }
            else
            {
                parser.BitBuffer.SeekBits((Int32)nBits);
            }
        }

        private void MessageUserMessage()
        {
            UInt32 id = parser.BitBuffer.ReadUnsignedBits(8);
            UInt32 length = parser.BitBuffer.ReadUnsignedBits(11);
            Int32 endPosition = parser.BitBuffer.CurrentBit + (Int32)length;

            if (id == 4) // SayText
            {
                Byte playerId = parser.BitBuffer.ReadByte();

                String token = parser.BitBuffer.ReadString();
                token = token.Replace((Char)0x01, '#');

                GameLogWriteTimestamp();
                GameLogWrite(Detokenise(token) + parser.BitBuffer.ReadString() + " :  " + parser.BitBuffer.ReadString() + "\r");
            }

            parser.BitBuffer.SeekBits(endPosition, SeekOrigin.Begin);
        }

        private void MessageGameEventList()
        {
            parser.MessageGameEventList();

            // add game event handlers
            parser.AddGameEventCallback("round_start", GameEventStartRound);
            parser.AddGameEventCallback("round_end", GameEventEndRound);
            parser.AddGameEventCallback("player_death", GameEventPlayerDeath);
            parser.AddGameEventCallback("bomb_planted", GameEventBombPlanted);
        }
        #endregion

        #region Game Events
        private void GameEventStartRound()
        {
            parser.BitBuffer.SeekBits(64); // time limit, frag limit
            parser.BitBuffer.ReadString(); // objective

            NewRound();
        }

        private void GameEventEndRound()
        {
            parser.BitBuffer.SeekBits(16); // winner, reason
            String message = parser.BitBuffer.ReadString();

            GameLogWriteTimestamp();
            GameLogWrite(Detokenise(message) + "\r");
        }

        private void GameEventPlayerDeath()
        {
            UInt32 victimId = parser.BitBuffer.ReadUnsignedBits(16);
            UInt32 attackerId = parser.BitBuffer.ReadUnsignedBits(16);
            String weaponName = parser.BitBuffer.ReadString();
            Boolean headshot = parser.BitBuffer.ReadBoolean();

            GameLogWriteTimestamp();

            if (weaponName == "world")
            {
                GameLogWrite(String.Format("{0} suicided.\r", FindPlayerName(victimId)));
            }
            else
            {
                GameLogWrite(String.Format("{0} killed {1} with {2}{3}.\r", FindPlayerName(attackerId), FindPlayerName(victimId), (headshot ? "a headshot from " : ""), weaponName));
            }
        }

        private void GameEventBombPlanted()
        {
            UInt32 playerId = parser.BitBuffer.ReadUnsignedBits(16);
            parser.BitBuffer.SeekBits(48); // site, posx, posy

            GameLogWriteTimestamp();
            GameLogWrite(String.Format("{0} planted the bomb.\r", FindPlayerName(playerId)));
        }
        #endregion
    }
}
