using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace compLexity_Demo_Player
{
    public class HalfLifeDemoConverter : IHalfLifeDemoWriter
    {
        // a svc_resourcelist entry
        private class Resource
        {
            public UInt32 type;
            public String name;
            public UInt32 index;
            public Int32 fileSize;
            public UInt32 flags;
            public Byte[] md5Hash;
            public Boolean hasReservedData;
            public Byte[] reservedData;
        }

        private HalfLifeDemo demo;
        private HalfLifeDemoParser parser;

        // network protocol conversion
        private Dictionary<String, Byte> compatibleUserMessages; // CS 1.6 user messages. when converting 1.3-1.5 demos to 1.6, user messages must use id's that are compatible with the mod dll's

        // FIXME: initialise to one above highest ID in compatibleUserMessages (then when assigning, check for overflow and if that happens, iterate through compatibleUserMessages for an available ID).
        private Byte freeUserMessage = 150; // the first freely available user message (so non-CS 1.6 user messages don't end up clashing with CS 1.6 id's)

        private UInt32 createSmokeEventIndex; // for CS 1.0-1.1 demos - different smoke event iparam1

        public HalfLifeDemoConverter(HalfLifeDemo demo)
        {
            this.demo = demo;

            // TODO: read this in from an XML file, and store it in a static class
            compatibleUserMessages = new Dictionary<String, Byte>();
            compatibleUserMessages.Add("HudTextArgs", 145);
            compatibleUserMessages.Add("ShowTimer", 144);
            compatibleUserMessages.Add("Fog", 143);
            compatibleUserMessages.Add("Brass", 142);
            compatibleUserMessages.Add("BotProgress", 141);
            compatibleUserMessages.Add("Location", 140);
            compatibleUserMessages.Add("ItemStatus", 139);
            compatibleUserMessages.Add("BarTime2", 138);
            compatibleUserMessages.Add("SpecHealth2", 137);
            compatibleUserMessages.Add("BuyClose", 136);
            compatibleUserMessages.Add("BotVoice", 135);
            compatibleUserMessages.Add("Scenario", 134);
            compatibleUserMessages.Add("TaskTime", 133);
            compatibleUserMessages.Add("ShadowIdx", 132);
            compatibleUserMessages.Add("CZCareerHUD", 131);
            compatibleUserMessages.Add("CZCareer", 130);
            compatibleUserMessages.Add("ReceiveW", 129);
            compatibleUserMessages.Add("ADStop", 128);
            compatibleUserMessages.Add("ForceCam", 127);
            compatibleUserMessages.Add("SpecHealth", 126);
            compatibleUserMessages.Add("HLTV", 125);
            compatibleUserMessages.Add("HostageK", 124);
            compatibleUserMessages.Add("HostagePos", 123);
            compatibleUserMessages.Add("ClCorpse", 122);
            compatibleUserMessages.Add("BombPickup", 121);
            compatibleUserMessages.Add("BombDrop", 120);
            compatibleUserMessages.Add("AllowSpec", 119);
            compatibleUserMessages.Add("TutorClose", 118);
            compatibleUserMessages.Add("TutorState", 117);
            compatibleUserMessages.Add("TutorLine", 116);
            compatibleUserMessages.Add("TutorText", 115);
            compatibleUserMessages.Add("VGUIMenu", 114);
            compatibleUserMessages.Add("Spectator", 113);
            compatibleUserMessages.Add("Radar", 112);
            compatibleUserMessages.Add("NVGToggle", 111);
            compatibleUserMessages.Add("Crosshair", 110);
            compatibleUserMessages.Add("ReloadSound", 109);
            compatibleUserMessages.Add("BarTime", 108);
            compatibleUserMessages.Add("StatusIcon", 107);
            compatibleUserMessages.Add("StatusText", 106);
            compatibleUserMessages.Add("StatusValue", 105);
            compatibleUserMessages.Add("BlinkAcct", 104);
            compatibleUserMessages.Add("ArmorType", 103);
            compatibleUserMessages.Add("Money", 102);
            compatibleUserMessages.Add("RoundTime", 101);
            compatibleUserMessages.Add("SendAudio", 100);
            compatibleUserMessages.Add("AmmoX", 99);
            compatibleUserMessages.Add("ScreenFade", 98);
            compatibleUserMessages.Add("ScreenShake", 97);
            compatibleUserMessages.Add("ShowMenu", 96);
            compatibleUserMessages.Add("SetFOV", 95);
            compatibleUserMessages.Add("HideWeapon", 94);
            compatibleUserMessages.Add("ItemPickup", 93);
            compatibleUserMessages.Add("WeapPickup", 92);
            compatibleUserMessages.Add("AmmoPickup", 91);
            compatibleUserMessages.Add("ServerName", 90);
            compatibleUserMessages.Add("MOTD", 89);
            compatibleUserMessages.Add("GameMode", 88);
            compatibleUserMessages.Add("TeamScore", 87);
            compatibleUserMessages.Add("TeamInfo", 86);
            compatibleUserMessages.Add("ScoreInfo", 85);
            compatibleUserMessages.Add("ScoreAttrib", 84);
            compatibleUserMessages.Add("DeathMsg", 83);
            compatibleUserMessages.Add("GameTitle", 82);
            compatibleUserMessages.Add("ViewMode", 81);
            compatibleUserMessages.Add("InitHUD", 80);
            compatibleUserMessages.Add("ResetHUD", 79);
            compatibleUserMessages.Add("WeaponList", 78);
            compatibleUserMessages.Add("TextMsg", 77);
            compatibleUserMessages.Add("SayText", 76);
            compatibleUserMessages.Add("HudText", 75);
            compatibleUserMessages.Add("HudTextPro", 74);
            compatibleUserMessages.Add("Train", 73);
            compatibleUserMessages.Add("Battery", 72);
            compatibleUserMessages.Add("Damage", 71);
            compatibleUserMessages.Add("Health", 70);
            compatibleUserMessages.Add("FlashBat", 69);
            compatibleUserMessages.Add("Flashlight", 68);
            compatibleUserMessages.Add("Geiger", 67);
            compatibleUserMessages.Add("CurWeapon", 66);
            compatibleUserMessages.Add("ReqState", 65);
            compatibleUserMessages.Add("VoiceMask", 64);
        }

        #region Interface
        public void AddMessageHandlers(HalfLifeDemoParser parser)
        {
            this.parser = parser;

            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_event, MessageEvent);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_sound, MessageSound);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_serverinfo, MessageServerInfo);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_updateuserinfo, MessageUpdateUserInfo);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_deltadescription, MessageDeltaDescription);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_clientdata, MessageClientData);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_pings, MessagePings);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_spawnbaseline, MessageSpawnBaseline);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_newusermsg, MessageNewUserMsg);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_packetentities, MessagePacketEntities);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_deltapacketentities, MessageDeltaPacketEntities);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_resourcelist, MessageResourceList);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_hltv, MessageHltv);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_voiceinit, MessageVoiceInit);
            parser.AddUserMessageHandler("ScreenFade", MessageScreenFade);
            parser.AddUserMessageHandler("SendAudio", MessageSendAudio);

            Procedure<String> removeMessage = (s) =>
            {
                Int32 startOffset = parser.BitBuffer.CurrentByte;
                Int32 messageLength = parser.FindUserMessageLength(s);
                Int32 endOffset = parser.BitBuffer.CurrentByte + messageLength;
                parser.Seek(startOffset - 1, SeekOrigin.Begin);
                parser.BitBuffer.RemoveBytes(endOffset - startOffset + 1);
            };

            parser.AddUserMessageHandler("CDChallenge", () =>
            {
                removeMessage("CDChallenge");
            });

            parser.AddUserMessageHandler("CDSalt", () =>
            {
                removeMessage("CDSalt");
            });
        }

        public void ProcessHeader(ref Byte[] header)
        {
            if (demo.ConvertToCurrentNetworkProtocol())
            {
                header[12] = HalfLifeDemo.CurrentNetworkProtocol;
            }
        }

        public Boolean ShouldParseGameDataMessages(Byte frameType)
        {
            return (demo.ConvertNetworkProtocol() || frameType == 0 || (demo.GameFolderName == "cstrike" && demo.Perspective == Demo.PerspectiveEnum.Pov && Config.Settings.PlaybackRemoveFtb));
        }

        public Boolean ShouldWriteClientCommand(String command)
        {
            if (Config.Settings.PlaybackRemoveShowscores && (command == "+showscores" || command == "-showscores"))
            {
                return false;
            }

            if (command.StartsWith("adjust_crosshair"))
            {
                return false;
            }

            return true;
        }

        public Byte GetNewUserMessageId(Byte messageId)
        {
            // beta steam demos, or any other network protocol conversion
            if (!demo.ConvertNetworkProtocol())
            {
                return messageId;
            }

            String name = parser.FindMessageIdString(messageId);

            if (!compatibleUserMessages.ContainsKey(name))
            {
                // shouldn't happen, must be a bad message
                // let the parser handle it
                return messageId;
            }

            return compatibleUserMessages[name];
        }

        public void WriteDemoInfo(Byte[] demoInfo, MemoryStream ms)
        {
            if (demo.ConvertNetworkProtocol() && demo.NetworkProtocol <= 43)
            {
                // zero out some data
                for (Int32 i = 28; i < 436; i++)
                {
                    demoInfo[i] = (Byte)0;
                }

                // move view model info
                for (Int32 i = 0; i < 15; i++)
                {
                    demoInfo[421 + i] = demoInfo[517 + i];
                }

                // copy only what we need (to match the length of the current network protocol)
                ms.Write(demoInfo, 0, 436);
            }
            else
            {
                ms.Write(demoInfo, 0, demoInfo.Length);
            }
        }
        #endregion

        private void ReWriteMessage(Int32 messageStartOffset, Byte[] data)
        {
            // remove old message
            Int32 messageEndOffset = parser.BitBuffer.CurrentByte;
            parser.Seek(messageStartOffset, SeekOrigin.Begin);
            parser.BitBuffer.RemoveBytes(messageEndOffset - messageStartOffset);

            // insert new message
            parser.BitBuffer.InsertBytes(data);
        }

        private void ConvertSequenceNumber(ref UInt32? sequence)
        {
            if (demo.GameVersion == HalfLifeDemo.GameVersionEnum.CounterStrike10)
            {
                if (sequence <= 6) // 0 to 6 map to 1 to 7
                {
                    sequence++; // no dummy in CS 1.0
                }
                else if (sequence >= 7 && sequence <= 79) // 7+ maps to 10+ until 79
                {
                    sequence += 3; // no dummy (as above) and no swim or treadwater sequences
                }
                else
                {
                    sequence += 19; // no shield sequences
                }
            }
            else // 1.1-1.5
            {
                if (sequence >= 83)
                {
                    sequence += 16;
                }
            }
        }

        #region Message Handlers
        private void MessageEvent()
        {
            if (!demo.ConvertNetworkProtocol() || demo.NetworkProtocol > 43)
            {
                parser.MessageEvent();
                return;
            }

            Int32 messageStartOffset = parser.BitBuffer.CurrentByte;

            // read message
            parser.BitBuffer.Endian = BitBuffer.EndianType.Big;

            BitWriter bitWriter = new BitWriter();
            HalfLifeDeltaStructure eventStructure = parser.GetDeltaStructure("event_t");

            UInt32 nEvents = parser.BitBuffer.ReadUnsignedBits(5);
            bitWriter.WriteUnsignedBits(nEvents, 5);

            for (Int32 i = 0; i < nEvents; i++)
            {
                UInt32 eventIndex = parser.BitBuffer.ReadUnsignedBits(10);
                bitWriter.WriteUnsignedBits(eventIndex, 10); // event index

                Boolean packetIndexBit = parser.BitBuffer.ReadBoolean();
                bitWriter.WriteBoolean(packetIndexBit);

                if (packetIndexBit)
                {
                    bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(11), 11); // packet index

                    Boolean deltaBit = parser.BitBuffer.ReadBoolean();
                    bitWriter.WriteBoolean(deltaBit);

                    if (deltaBit)
                    {
                        HalfLifeDelta delta = eventStructure.CreateDelta();
                        Byte[] bitmaskBytes;
                        eventStructure.ReadDelta(parser.BitBuffer, delta, out bitmaskBytes);

                        // smoke fix for CS 1.0-1.1 demos
                        if (eventIndex == createSmokeEventIndex)
                        {
                            if (delta.FindEntryValue("iparam1") != null)
                            {
                                Random r = new Random();
                                delta.SetEntryValue("iparam1", r.Next(128, 300)); // FIXME: random guess
                            }
                        }

                        eventStructure.WriteDelta(bitWriter, delta, bitmaskBytes);
                    }
                }

                Boolean fireTimeBit = parser.BitBuffer.ReadBoolean();
                bitWriter.WriteBoolean(fireTimeBit);

                if (fireTimeBit)
                {
                    bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(16), 16); // fire time
                }
            }

            parser.BitBuffer.SkipRemainingBits();
            parser.BitBuffer.Endian = BitBuffer.EndianType.Little;

            // insert new message
            ReWriteMessage(messageStartOffset, bitWriter.Data);
        }

        private void MessageSound()
        {
            if (!demo.ConvertNetworkProtocol() || demo.NetworkProtocol > 43)
            {
                parser.MessageSound();
                return;
            }

            Int32 messageStartOffset = parser.BitBuffer.CurrentByte;

            // read message
            parser.BitBuffer.Endian = BitBuffer.EndianType.Big;

            BitWriter bitWriter = new BitWriter();

            UInt32 flags = parser.BitBuffer.ReadUnsignedBits(9);
            bitWriter.WriteUnsignedBits(flags, 9);

            if ((flags & (1 << 0)) != 0) // volume
            {
                bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(8), 8);
            }

            if ((flags & (1 << 1)) != 0) // attenuation * 64
            {
                bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(8), 8);
            }

            bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(3), 3); // channel
            bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(11), 11); // edict number

            if ((flags & (1 << 2)) != 0) // sound index (short)
            {
                bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(16), 16);
            }
            else // sound index (byte)
            {
                bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(8), 8);
            }

            bitWriter.WriteVectorCoord(true, parser.BitBuffer.ReadVectorCoord(true)); // position

            if ((flags & (1 << 3)) != 0) // pitch
            {
                bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(8), 8);
            }

            parser.BitBuffer.SkipRemainingBits();
            parser.BitBuffer.Endian = BitBuffer.EndianType.Little;

            // insert new message
            ReWriteMessage(messageStartOffset, bitWriter.Data);
        }

        private void MessageServerInfo()
        {
            if (demo.ConvertToCurrentNetworkProtocol())
            {
                parser.BitBuffer.RemoveBytes(4);
                parser.BitBuffer.InsertBytes(new Byte[] { (Byte)HalfLifeDemo.CurrentNetworkProtocol, 0, 0, 0 });
            }
            else
            {
                parser.Seek(4); // network protocol
            }

            parser.Seek(4); // process count
            parser.Seek(4); // munged map checksum
            parser.Seek(16); // client.dll checksum
            parser.Seek(1); // max clients
            parser.Seek(1); // recorder slot
            parser.Seek(1); // deathmatch/coop flag
            parser.BitBuffer.ReadString(); // game folder

            // server name
            if (demo.NetworkProtocol > 43)
            {
                parser.BitBuffer.ReadString();
            }
            else
            {
                if (demo.ConvertNetworkProtocol())
                {
                    Byte[] serverNameBytes = Encoding.ASCII.GetBytes(demo.ServerName);
                    parser.BitBuffer.InsertBytes(serverNameBytes);
                    parser.BitBuffer.InsertBytes(new Byte[] { (Byte)0 }); // null terminator
                }
            }

            // skip map
            parser.BitBuffer.ReadString();

            if (demo.NetworkProtocol == 45)
            {
                Byte extraInfo = parser.BitBuffer.ReadByte();
                parser.Seek(-1);

                if (extraInfo == (Byte)HalfLifeDemoParser.MessageId.svc_sendextrainfo)
                {
                    goto InsertMapCycle;
                }
            }

            parser.BitBuffer.ReadString(); // skip mapcycle

            if (demo.NetworkProtocol <= 43)
            {
                goto InsertExtraFlag;
            }

            if (parser.BitBuffer.ReadByte() > 0)
            {
                parser.Seek(-1);
                parser.BitBuffer.RemoveBytes(demo.NetworkProtocol == 45 ? 34 : 22);
                goto InsertExtraFlag;
            }
            else
            {
                return;
            }

            InsertMapCycle:
                parser.BitBuffer.InsertBytes(new Byte[] { 0 });

            InsertExtraFlag:
                parser.BitBuffer.InsertBytes(new Byte[] { 0 });
        }

        private void MessageUpdateUserInfo()
        {
            parser.MessageUpdateUserInfo();

            // insert empty string hash
            if (demo.NetworkProtocol <= 43 && demo.ConvertNetworkProtocol())
            {
                parser.BitBuffer.InsertBytes(new Byte[16]);
            }
        }

        private void MessageDeltaDescription()
        {
            if (!demo.ConvertNetworkProtocol() || demo.NetworkProtocol > 43)
            {
                parser.MessageDeltaDescription();
                return;
            }

            Int32 messageStartOffset = parser.BitBuffer.CurrentByte;
            parser.BitBuffer.Endian = BitBuffer.EndianType.Big;
            BitWriter bitWriter = new BitWriter();

            // read/write message
            String structureName = parser.BitBuffer.ReadString();
            bitWriter.WriteString(structureName);

            UInt32 nEntries = parser.BitBuffer.ReadUnsignedBits(16);
            bitWriter.WriteUnsignedBits(nEntries, 16);

            HalfLifeDeltaStructure newDeltaStructure = new HalfLifeDeltaStructure(structureName);
            parser.AddDeltaStructure(newDeltaStructure);

            HalfLifeDeltaStructure deltaDescription = parser.GetDeltaStructure("delta_description_t");

            for (UInt16 i = 0; i < nEntries; i++)
            {
                HalfLifeDelta delta = deltaDescription.CreateDelta();
                Byte[] bitmaskBytes;
                deltaDescription.ReadDelta(parser.BitBuffer, delta, out bitmaskBytes);
                deltaDescription.WriteDelta(bitWriter, delta, bitmaskBytes);
                newDeltaStructure.AddEntry(delta);
            }

            parser.BitBuffer.SkipRemainingBits();
            parser.BitBuffer.Endian = BitBuffer.EndianType.Little;

            // insert new message
            ReWriteMessage(messageStartOffset, bitWriter.Data);
        }

        private void MessageClientData()
        {
            if (demo.Perspective == Demo.PerspectiveEnum.Hltv)
            {
                return;
            }

            if (!demo.ConvertNetworkProtocol() || demo.IsBetaSteam())
            {
                parser.MessageClientData();
                return;
            }

            // read message
            Int32 messageStartOffset = parser.BitBuffer.CurrentByte;

            if (demo.NetworkProtocol <= 43)
            {
                parser.BitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            BitWriter bitWriter = new BitWriter();
            HalfLifeDeltaStructure clientDataStructure = parser.GetDeltaStructure("clientdata_t");
            HalfLifeDeltaStructure weaponDataStructure = parser.GetDeltaStructure("weapon_data_t");

            Boolean deltaSequence = parser.BitBuffer.ReadBoolean();
            bitWriter.WriteBoolean(deltaSequence);

            UInt32 deltaSequenceNumber = 0;

            if (deltaSequence)
            {
                deltaSequenceNumber = parser.BitBuffer.ReadUnsignedBits(8);
                bitWriter.WriteUnsignedBits(deltaSequenceNumber, 8);
            }

            HalfLifeDelta clientData = clientDataStructure.CreateDelta();
            Byte[] clientDataBitmaskBytes;
            clientDataStructure.ReadDelta(parser.BitBuffer, clientData, out clientDataBitmaskBytes);
            clientDataStructure.WriteDelta(bitWriter, clientData, clientDataBitmaskBytes);

            while (parser.BitBuffer.ReadBoolean())
            {
                bitWriter.WriteBoolean(true);

                if (demo.NetworkProtocol < 47 && !demo.IsBetaSteam())
                {
                    bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(5), 6);
                }
                else
                {
                    bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(6), 6);
                }

                HalfLifeDelta weaponData = weaponDataStructure.CreateDelta();
                Byte[] bitmaskBytes;
                weaponDataStructure.ReadDelta(parser.BitBuffer, weaponData, out bitmaskBytes);
                weaponDataStructure.WriteDelta(bitWriter, weaponData, bitmaskBytes);
            }

            bitWriter.WriteBoolean(false);

            parser.BitBuffer.SkipRemainingBits();
            parser.BitBuffer.Endian = BitBuffer.EndianType.Little;

            // insert new message
            ReWriteMessage(messageStartOffset, bitWriter.Data);
        }

        private void MessagePings()
        {
            if (!demo.ConvertNetworkProtocol() || demo.NetworkProtocol > 43)
            {
                parser.MessagePings();
                return;
            }

            // read into new message
            Int32 messageStartOffset = parser.BitBuffer.CurrentByte;
            BitWriter bitWriter = new BitWriter();
            parser.BitBuffer.Endian = BitBuffer.EndianType.Big;

            while (parser.BitBuffer.ReadBoolean())
            {
                bitWriter.WriteBoolean(true);
                bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(5), 5); // slot
                bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(12), 12); // ping
                bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(7), 7); // loss
            }

            bitWriter.WriteBoolean(false);

            parser.BitBuffer.SkipRemainingBits();
            parser.BitBuffer.Endian = BitBuffer.EndianType.Little;

            // insert new message
            ReWriteMessage(messageStartOffset, bitWriter.Data);
        }

        private void MessageSpawnBaseline()
        {
            if (!demo.ConvertNetworkProtocol() || demo.NetworkProtocol > 43)
            {
                parser.MessageSpawnBaseline();
                return;
            }

            Int32 messageStartOffset = parser.BitBuffer.CurrentByte;
            BitWriter bitWriter = new BitWriter();

            // read message into new message
            parser.BitBuffer.Endian = BitBuffer.EndianType.Big;

            while (true)
            {
                UInt32 entityIndex = parser.BitBuffer.ReadUnsignedBits(11);
                bitWriter.WriteUnsignedBits(entityIndex, 11);

                if (entityIndex == (1 << 11) - 1) // all 1's
                {
                    break;
                }

                UInt32 entityType = parser.BitBuffer.ReadUnsignedBits(2);
                bitWriter.WriteUnsignedBits(entityType, 2);

                String entityTypeString;

                if ((entityType & 1) != 0)
                {
                    if (entityIndex > 0 && entityIndex <= demo.MaxClients)
                    {
                        entityTypeString = "entity_state_player_t";
                    }
                    else
                    {
                        entityTypeString = "entity_state_t";
                    }
                }
                else
                {
                    entityTypeString = "custom_entity_state_t";
                }

                HalfLifeDeltaStructure deltaStructure = parser.GetDeltaStructure(entityTypeString);
                HalfLifeDelta delta = deltaStructure.CreateDelta();
                Byte[] bitmaskBytes;
                deltaStructure.ReadDelta(parser.BitBuffer, delta, out bitmaskBytes);

                if (demo.GameVersion >= HalfLifeDemo.GameVersionEnum.CounterStrike10 && demo.GameVersion <= HalfLifeDemo.GameVersionEnum.CounterStrike15)
                {
                    if (entityTypeString == "entity_state_player_t")
                    {
                        UInt32? sequence = (UInt32?)delta.FindEntryValue("sequence");

                        if (sequence != null)
                        {
                            ConvertSequenceNumber(ref sequence);
                            delta.SetEntryValue("sequence", sequence);
                        }

                        UInt32? gaitSequence = (UInt32?)delta.FindEntryValue("gaitsequence");

                        if (gaitSequence != null)
                        {
                            ConvertSequenceNumber(ref gaitSequence);
                            delta.SetEntryValue("gaitsequence", gaitSequence);
                        }
                    }
                }

                deltaStructure.WriteDelta(bitWriter, delta, bitmaskBytes);
            }

            UInt32 footer = parser.BitBuffer.ReadUnsignedBits(5); // should be all 1's
            bitWriter.WriteUnsignedBits(footer, 5);

            if (footer != (1 << 5) - 1)
            {
                throw new ApplicationException("Bad svc_spawnbaseline footer.");
            }

            UInt32 nExtraData = parser.BitBuffer.ReadUnsignedBits(6);
            bitWriter.WriteUnsignedBits(nExtraData, 6);

            HalfLifeDeltaStructure entityStateStructure = parser.GetDeltaStructure("entity_state_t");

            for (Int32 i = 0; i < nExtraData; i++)
            {
                HalfLifeDelta delta = entityStateStructure.CreateDelta();
                Byte[] bitmaskBytes;
                entityStateStructure.ReadDelta(parser.BitBuffer, delta, out bitmaskBytes);
                entityStateStructure.WriteDelta(bitWriter, delta, bitmaskBytes);
            }

            parser.BitBuffer.Endian = BitBuffer.EndianType.Little;
            parser.BitBuffer.SkipRemainingBits();

            // insert new message
            ReWriteMessage(messageStartOffset, bitWriter.Data);
        }

        private void MessageNewUserMsg()
        {
            Int32 messageStartOffset = parser.BitBuffer.CurrentByte;

            // read message
            Byte id = parser.BitBuffer.ReadByte();
            SByte length = parser.BitBuffer.ReadSByte();
            String name = parser.BitBuffer.ReadString(16);

            parser.AddUserMessage(id, length, name);

            // FIXME: clean this up
            if (!demo.ConvertNetworkProtocol() && name != "CDChallenge" && name != "CDSalt")
            {
                return;
            }

            Byte newId;

            if (compatibleUserMessages.ContainsKey(name))
            {
                newId = compatibleUserMessages[name];
            }
            else
            {
                // cheating death
                // TODO: probably should have a list of "bad" user messages to remove...
                // TODO: should remove these messages even when not converting network protocols
                if (name == "CDChallenge" || name == "CDSalt")
                {
                    // remove message
                    Int32 messageFinishOffset = parser.BitBuffer.CurrentByte;
                    parser.BitBuffer.SeekBytes(messageStartOffset - 1, SeekOrigin.Begin);
                    parser.BitBuffer.RemoveBytes(messageFinishOffset - messageStartOffset + 1); // +1 for message id
                    return;
                }

                // user message doesn't exist in CS 1.6. shouldn't happen, but meh...
                // TODO: use an id unused by compatibleUserMessageTable
                //newId = (Byte?)id;
                newId = freeUserMessage;
                freeUserMessage++;

                compatibleUserMessages.Add(name, newId);
            }

            BitWriter bitWriter = new BitWriter();
            bitWriter.WriteByte((Byte)newId);
            bitWriter.WriteSByte(length);
            bitWriter.WriteString(name, 16);

            // insert new message
            ReWriteMessage(messageStartOffset, bitWriter.Data);
        }

        private void MessagePacketEntities()
        {
            if (!demo.ConvertNetworkProtocol() || demo.NetworkProtocol > 43)
            {
                parser.MessagePacketEntities();
                return;
            }

            Int32 messageStartOffset = parser.BitBuffer.CurrentByte;
            BitWriter bitWriter = new BitWriter();

            // read message into new message
            bitWriter.WriteUInt16(parser.BitBuffer.ReadUInt16()); // nEntities/maxEntities

            parser.BitBuffer.Endian = BitBuffer.EndianType.Big;

            UInt32 entityNumber = 0;

            while (true)
            {
                UInt16 footer = parser.BitBuffer.ReadUInt16();

                if (footer == 0)
                {
                    bitWriter.WriteUInt16(footer);
                    break;
                }
                else
                {
                    parser.BitBuffer.SeekBits(-16);
                }

                if (!parser.BitBuffer.ReadBoolean()) // entity number isn't last entity number + 1, need to read it in
                {
                    bitWriter.WriteBoolean(false);

                    // is the following entity number absolute, or relative from the last one?
                    if (parser.BitBuffer.ReadBoolean())
                    {
                        bitWriter.WriteBoolean(true);
                        entityNumber = parser.BitBuffer.ReadUnsignedBits(11);
                        bitWriter.WriteUnsignedBits(entityNumber, 11);
                    }
                    else
                    {
                        bitWriter.WriteBoolean(false);
                        UInt32 entityNumberDelta = parser.BitBuffer.ReadUnsignedBits(6);
                        bitWriter.WriteUnsignedBits(entityNumberDelta, 6);
                        entityNumber += entityNumberDelta;
                    }
                }
                else
                {
                    bitWriter.WriteBoolean(true);
                    entityNumber++;
                }

                Boolean custom = parser.BitBuffer.ReadBoolean();
                bitWriter.WriteBoolean(custom);
                Boolean baseline = parser.BitBuffer.ReadBoolean();
                bitWriter.WriteBoolean(baseline);

                if (baseline)
                {
                    bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(6), 6); // baseline index
                }

                String entityType = "entity_state_t";

                if (entityNumber > 0 && entityNumber <= demo.MaxClients)
                {
                    entityType = "entity_state_player_t";
                }
                else if (custom)
                {
                    entityType = "custom_entity_state_t";
                }

                HalfLifeDeltaStructure entityStateStructure = parser.GetDeltaStructure(entityType);
                HalfLifeDelta delta = entityStateStructure.CreateDelta();
                Byte[] bitmaskBytes;
                entityStateStructure.ReadDelta(parser.BitBuffer, delta, out bitmaskBytes);

                if (demo.GameVersion >= HalfLifeDemo.GameVersionEnum.CounterStrike10 && demo.GameVersion <= HalfLifeDemo.GameVersionEnum.CounterStrike15)
                {
                    if (entityType == "entity_state_player_t")
                    {
                        UInt32? sequence = (UInt32?)delta.FindEntryValue("sequence");

                        if (sequence != null)
                        {
                            ConvertSequenceNumber(ref sequence);
                            delta.SetEntryValue("sequence", sequence);
                        }

                        UInt32? gaitSequence = (UInt32?)delta.FindEntryValue("gaitsequence");

                        if (gaitSequence != null)
                        {
                            ConvertSequenceNumber(ref gaitSequence);
                            delta.SetEntryValue("gaitsequence", gaitSequence);
                        }
                    }
                }

                entityStateStructure.WriteDelta(bitWriter, delta, bitmaskBytes);
            }

            parser.BitBuffer.SkipRemainingBits();
            parser.BitBuffer.Endian = BitBuffer.EndianType.Little;

            // insert new message
            ReWriteMessage(messageStartOffset, bitWriter.Data);
        }

        private void MessageDeltaPacketEntities()
        {
            if (!demo.ConvertNetworkProtocol() || demo.IsBetaSteam())
            {
                parser.MessageDeltaPacketEntities();
                return;
            }

            Int32 messageStartOffset = parser.BitBuffer.CurrentByte;
            BitWriter bitWriter = new BitWriter();

            // read message
            bitWriter.WriteUInt16(parser.BitBuffer.ReadUInt16()); // nEntities/maxEntities

            if (demo.NetworkProtocol <= 43)
            {
                parser.BitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            bitWriter.WriteByte(parser.BitBuffer.ReadByte()); // delta sequence number

            UInt32 entityNumber = 0;

            while (true)
            {
                // check for footer
                UInt16 footer = parser.BitBuffer.ReadUInt16();

                if (footer == 0)
                {
                    bitWriter.WriteUInt16(footer);
                    break;
                }

                parser.BitBuffer.SeekBits(-16);

                // option bits
                Boolean removeEntity = parser.BitBuffer.ReadBoolean();
                bitWriter.WriteBoolean(removeEntity);
                Boolean absoluteEntityNumber = parser.BitBuffer.ReadBoolean();
                bitWriter.WriteBoolean(absoluteEntityNumber);

                // entity number
                if (absoluteEntityNumber)
                {
                    entityNumber = parser.BitBuffer.ReadUnsignedBits(11);
                    bitWriter.WriteUnsignedBits(entityNumber, 11);
                }
                else
                {
                    UInt32 deltaEntityNumber = parser.BitBuffer.ReadUnsignedBits(6);
                    bitWriter.WriteUnsignedBits(deltaEntityNumber, 6);
                    entityNumber += deltaEntityNumber;
                }

                if (!removeEntity)
                {
                    // entity type
                    Boolean custom = parser.BitBuffer.ReadBoolean();
                    bitWriter.WriteBoolean(custom);

                    if (demo.NetworkProtocol <= 43)
                    {
                        parser.BitBuffer.SeekBits(1); // unknown, always 0
                    }

                    String entityType = "entity_state_t";

                    if (entityNumber > 0 && entityNumber <= demo.MaxClients)
                    {
                        entityType = "entity_state_player_t";
                    }
                    else if (custom)
                    {
                        entityType = "custom_entity_state_t";
                    }

                    // delta compressed data
                    Byte[] bitmaskBytes;
                    HalfLifeDeltaStructure deltaDecoder = parser.GetDeltaStructure(entityType);
                    HalfLifeDelta deltaEntity = deltaDecoder.CreateDelta();
                    deltaDecoder.ReadDelta(parser.BitBuffer, deltaEntity, out bitmaskBytes);

                    if (demo.GameVersion >= HalfLifeDemo.GameVersionEnum.CounterStrike10 && demo.GameVersion <= HalfLifeDemo.GameVersionEnum.CounterStrike15)
                    {
                        if (entityType == "entity_state_player_t")
                        {
                            UInt32? sequence = (UInt32?)deltaEntity.FindEntryValue("sequence");

                            if (sequence != null)
                            {
                                ConvertSequenceNumber(ref sequence);
                                deltaEntity.SetEntryValue("sequence", sequence);
                            }

                            UInt32? gaitSequence = (UInt32?)deltaEntity.FindEntryValue("gaitsequence");

                            if (gaitSequence != null)
                            {
                                ConvertSequenceNumber(ref gaitSequence);
                                deltaEntity.SetEntryValue("gaitsequence", gaitSequence);
                            }
                        }

                        // all entities: zero out animtime
                        if (demo.GameVersion == HalfLifeDemo.GameVersionEnum.CounterStrike10 || demo.GameVersion == HalfLifeDemo.GameVersionEnum.CounterStrike11)
                        {
                            if (deltaEntity.FindEntryValue("animtime") != null)
                            {
                                deltaEntity.SetEntryValue("animtime", 0.0f);
                            }
                        }
                    }

                    deltaDecoder.WriteDelta(bitWriter, deltaEntity, bitmaskBytes);
                }
            }

            parser.BitBuffer.SkipRemainingBits();
            parser.BitBuffer.Endian = BitBuffer.EndianType.Little;

            // insert new message
            ReWriteMessage(messageStartOffset, bitWriter.Data);
        }

        private void MessageResourceList()
        {
            Boolean shadowSpriteFound = false;
            const String shadowSpriteName = "sprites/shadow_circle.spr";

            // black list
            // TODO: same deal as compatiable user messages - read from xml file and put in static class

            // for .net framework 3.0 backward compatibility: no HashSet or Set collections exist
            //HashSet<String> resourceBlackList = new HashSet<String>();
            System.Collections.Hashtable resourceBlackList = new System.Collections.Hashtable();
            resourceBlackList.Add("sprites/top.spr", 1);
            resourceBlackList.Add("sprites/top2.spr", 1);
            resourceBlackList.Add("sprites/top3.spr", 1);
            resourceBlackList.Add("sprites/top_left.spr", 1);
            resourceBlackList.Add("sprites/top_left2.spr", 1);
            resourceBlackList.Add("sprites/top_left3.spr", 1);
            resourceBlackList.Add("sprites/top_right.spr", 1);
            resourceBlackList.Add("sprites/top_right2.spr", 1);
            resourceBlackList.Add("sprites/top_right3.spr", 1);
            resourceBlackList.Add("sprites/bottom.spr", 1);
            resourceBlackList.Add("sprites/bottom2.spr", 1);
            resourceBlackList.Add("sprites/bottom3.spr", 1);
            resourceBlackList.Add("sprites/bottom_left.spr", 1);
            resourceBlackList.Add("sprites/bottom_left2.spr", 1);
            resourceBlackList.Add("sprites/bottom_left3.spr", 1);
            resourceBlackList.Add("sprites/bottom_right.spr", 1);
            resourceBlackList.Add("sprites/bottom_right2.spr", 1);
            resourceBlackList.Add("sprites/bottom_right3.spr", 1);
            resourceBlackList.Add("sprites/left3.spr", 1);
            resourceBlackList.Add("sprites/right.spr", 1);
            resourceBlackList.Add("sprites/right2.spr", 1);
            resourceBlackList.Add("sprites/right3.spr", 1);
            resourceBlackList.Add("sprites/horizontal.spr", 1);
            resourceBlackList.Add("sprites/vertical.spr", 1);

            Int32 startByteIndex = parser.BitBuffer.CurrentByte;

            // read message
            if (demo.NetworkProtocol <= 43)
            {
                parser.BitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            UInt32 nEntries = parser.BitBuffer.ReadUnsignedBits(12);
            List<Resource> resourceList = new List<Resource>((Int32)nEntries);

            for (Int32 i = 0; i < nEntries; i++)
            {
                Resource r = new Resource();

                r.type = parser.BitBuffer.ReadUnsignedBits(4);
                r.name = parser.BitBuffer.ReadString();
                r.index = parser.BitBuffer.ReadUnsignedBits(12);
                r.fileSize = parser.BitBuffer.ReadBits(24); // signed?
                r.flags = parser.BitBuffer.ReadUnsignedBits(3);

                if ((r.flags & 4) != 0) // md5 hash (RES_CUSTOM?)
                {
                    r.md5Hash = parser.BitBuffer.ReadBytes(16);
                }

                r.hasReservedData = parser.BitBuffer.ReadBoolean();

                if (r.hasReservedData)
                {
                    r.reservedData = parser.BitBuffer.ReadBytes(32);
                }

                if (r.name.EndsWith("_r.mdl"))
                {
                    r.name = r.name.Replace("_r.mdl", ".mdl"); // seems to work fine...
                }

                if (resourceBlackList.Contains(r.name) == false)
                {
                    resourceList.Add(r);
                }

                if (r.type == 5 && r.name == "events/createsmoke.sc")
                {
                    createSmokeEventIndex = r.index;
                }

                if (r.type == 2 && r.name == shadowSpriteName)
                {
                    shadowSpriteFound = true;
                }
            }

            // consistency list
            // indices of resources to force consistency upon?
            if (parser.BitBuffer.ReadBoolean())
            {
                while (parser.BitBuffer.ReadBoolean())
                {
                    Int32 nBits = (parser.BitBuffer.ReadBoolean() ? 5 : 10);
                    parser.BitBuffer.SeekBits(nBits);
                }
            }

            parser.BitBuffer.SkipRemainingBits();
            parser.BitBuffer.Endian = BitBuffer.EndianType.Little;

            // TODO: wrong map fix
            // check for bsp extension (check r.type first? cuts down on string compares)
            // need to remove brush entities too... (check r.type as well?) do brush entities always precede bsp file?

            // stop now if we're not converting network protocols
            if (!demo.ConvertNetworkProtocol() || demo.IsBetaSteam())
            {
                return;
            }

            // create new message
            BitWriter bitWriter = new BitWriter();

            bitWriter.WriteUnsignedBits((UInt32)(resourceList.Count + (shadowSpriteFound ? 0 : 1)), 12);

            foreach (Resource r in resourceList)
            {
                bitWriter.WriteUnsignedBits(r.type, 4);
                bitWriter.WriteString(r.name);
                bitWriter.WriteUnsignedBits(r.index, 12);
                bitWriter.WriteBits(r.fileSize, 24);
                bitWriter.WriteUnsignedBits(r.flags, 3);

                if ((r.flags & 4) != 0) // md5 hash
                {
                    bitWriter.WriteBytes(r.md5Hash);
                }

                bitWriter.WriteBoolean(r.hasReservedData);

                if (r.hasReservedData)
                {
                    bitWriter.WriteBytes(r.reservedData);
                }
            }

            // insert shadow sprite if it doesn't exist
            if (!shadowSpriteFound)
            {
                bitWriter.WriteUnsignedBits(2, 4); // type
                bitWriter.WriteString(shadowSpriteName); // name
                bitWriter.WriteUnsignedBits(0, 12); // index
                bitWriter.WriteBits(4926, 24); // file size
                bitWriter.WriteUnsignedBits(1, 3); // flags
                bitWriter.WriteBoolean(false); // has reserved data
            }

            bitWriter.WriteBoolean(false); // consistency list

            // remove old message
            Int32 endByteIndex = parser.BitBuffer.CurrentByte;
            parser.Seek(startByteIndex, SeekOrigin.Begin);
            parser.BitBuffer.RemoveBytes(endByteIndex - startByteIndex);

            // insert new message into bitbuffer
            parser.BitBuffer.InsertBytes(bitWriter.Data);
        }

        // TODO: this needs work
        private void MessageHltv()
        {
            //perspective = PerspectiveEnum.Hltv;

            /*
            #define HLTV_ACTIVE				0	// tells client that he's an spectator and will get director commands
            #define HLTV_STATUS				1	// send status infos about proxy 
            #define HLTV_LISTEN				2	// tell client to listen to a multicast stream
             */

            Byte subCommand = parser.BitBuffer.ReadByte();

            if (subCommand == 2) // HLTV_LISTEN
            {
                // remove entire message
                parser.Seek(-2);
                parser.BitBuffer.RemoveBytes(10);
                //bitBuffer.InsertBytes(bitBuffer.GetNumBytesRead(), new Byte[] { 0 });
                //parser.Seek(8);
            }
            else if (subCommand == 1)
            {
                // TODO: fix this
                //MessageBox.Show("HLTV_STATUS");
            }
        }


        private void MessageVoiceInit()
        {
            parser.BitBuffer.ReadString();

            if (!demo.ConvertNetworkProtocol() || demo.IsBetaSteam())
            {
                if (demo.NetworkProtocol >= 47 || demo.IsBetaSteam())
                {
                    parser.Seek(1);
                }
            }
            else
            {
                parser.BitBuffer.InsertBytes(new Byte[] { 5 });
            }
        }

        private void MessageScreenFade()
        {
            Int32 startByteIndex = parser.BitBuffer.CurrentByte;

            UInt16 duration = parser.BitBuffer.ReadUInt16();
            UInt16 holdTime = parser.BitBuffer.ReadUInt16();
            UInt16 flags = parser.BitBuffer.ReadUInt16();
            UInt32 colour = parser.BitBuffer.ReadUInt32();

            if (!Config.Settings.PlaybackRemoveFtb)
            {
                return;
            }

            // see if it's fade to black
            // flags: FFADE_OUT | FFADE_STAYOUT
            // could probably just check flags and colour...
            if (duration == 0x3000 && holdTime == 0x3000 && flags == 0x05 && colour == 0xFF000000)
            {
                // remove the entire message
                parser.Seek(startByteIndex - 1, SeekOrigin.Begin);
                parser.BitBuffer.RemoveBytes(11);
            }
        }

        private void MessageSendAudio()
        {
            Byte length = parser.BitBuffer.ReadByte();
            Int32 startOffset = parser.BitBuffer.CurrentByte;

            Byte slot = parser.BitBuffer.ReadByte();
            //String name = parser.BitBuffer.ReadString();

            parser.Seek(startOffset, SeekOrigin.Begin);

            if (demo.ConvertNetworkProtocol())
            {
                // add 2 to length
                parser.Seek(-1);
                parser.BitBuffer.RemoveBytes(1);
                parser.BitBuffer.InsertBytes(new Byte[] { (Byte)(length + 2) });

                // append pitch (short, 100)
                parser.Seek(length);
                parser.BitBuffer.InsertBytes(new Byte[] { (Byte)100, (Byte)0 });
            }
            else
            {
                parser.Seek(length);
            }
        }
        #endregion

    }
}
