using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace compLexity_Demo_Player.Games
{
    public class CounterStrike : Game
    {
        public enum Versions
        {
            Unknown = 0,
            CounterStrike10,
            CounterStrike11,
            CounterStrike13,
            CounterStrike14,
            CounterStrike15,
            CounterStrike16
        }

        public CounterStrike()
        {
            Engine = Engines.HalfLife;
            AppId = 10;
            Folder = "cstrike";
            FolderExtended = "counter-strike";
            Name = "Counter-Strike";
            ReadConfig();
        }

        public override Int32 FindVersion(string checksum)
        {
            if (versions == null)
            {
                return 0;
            }

            if (versions.ContainsKey(checksum))
            {
                // Translate the string names defined in the XML config file (e.g. "1.3") to the enumeration Versions (e.g. "CounterStrike13").
                return (Int32)Enum.Parse(typeof(Versions), "CounterStrike" + versions[checksum].Replace(".", String.Empty));
            }

            return (Int32)Versions.CounterStrike16;
        }

        public override bool IsBetaSteamHltvDemo(HalfLifeDemo demo)
        {
            return (demo.GameVersion == (Int32)Versions.CounterStrike16);
        }

        public override bool CanAnalyse(Demo demo)
        {
            return true;
        }

        public override bool CanConvertNetworkProtocol(Demo demo)
        {
            return (demo.NetworkProtocol >= 43 && demo.NetworkProtocol <= 46 && !((HalfLifeDemo)demo).IsBetaSteam());
        }

        public override bool CanRemoveFadeToBlack(Demo demo)
        {
            return true;
        }

        public override bool CanRemoveHltvAds(Demo demo)
        {
            return true;
        }

        public override bool CanRemoveHltvSlowMotion(Demo demo)
        {
            return true;
        }

        public override SolidColorBrush TeamColour(string team)
        {
            if (team == "CT")
            {
                return Brushes.Blue;
            }
            else if (team == "TERRORIST")
            {
                return Brushes.Red;
            }

            return base.TeamColour(team);
        }

        public override int NewRoundEventId(Demo demo)
        {
            if (demo.GameVersion == (Int32)Versions.CounterStrike16)
            {
                return 29;
            }

            return 26;
        }

        #region Demo conversion
        public override Boolean ConvertResourceListCallback(Demo demo, UInt32 type, UInt32 index, ref String name)
        {
            if (demo.GameVersion == (Int32)Versions.CounterStrike16)
            {
                return true;
            }

            if (type == 5 && name == "events/createsmoke.sc")
            {
                // Store the create smoke event index for later so it can be identified when parsing the svc_event message.
                if (demo.Resources.ContainsKey(name))
                {
                    demo.Resources.Remove(name);
                }

                demo.Resources.Add(name, index);
            }

            if (resourceBlacklist.Contains(name))
            {
                return false;
            }

            // Old CS versions use seperate models for left and right handed weapon models.
            if (name.EndsWith("_r.mdl"))
            {
                name = name.Replace("_r.mdl", ".mdl");
            }

            return true;
        }

        public override void ConvertEventCallback(Demo demo, HalfLifeDelta delta, uint eventIndex)
        {
            // Smoke fix for CS 1.0-1.1 demos.
            if ((demo.GameVersion == (Int32)Versions.CounterStrike10 || demo.GameVersion == (Int32)Versions.CounterStrike11) && demo.Resources.ContainsKey("events/createsmoke.sc"))
            {
                if (eventIndex == demo.Resources["events/createsmoke.sc"])
                {
                    if (delta.FindEntryValue("iparam1") != null)
                    {
                        Random r = new Random();
                        delta.SetEntryValue("iparam1", r.Next(128, 300)); // FIXME: random guess
                    }
                }
            }

            // See ConvertDeltaDescriptionCallback.
            if (demo.GameVersion == (Int32)Versions.CounterStrike13)
            {
                for (int i = 1; i <= 2; i++)
                {
                    String paramName = "iparam" + i.ToString();
                    Int32? value = (Int32?)delta.FindEntryValue(paramName);

                    if (value != null)
                    {
                        delta.SetEntryValue(paramName, value / 8192);
                    }
                }
            }
        }

        public override void ConvertPacketEntititiesCallback(HalfLifeDelta delta, String entityType, Int32 gameVersion)
        {
            if (gameVersion < (Int32)Versions.CounterStrike10 || gameVersion > (Int32)Versions.CounterStrike15)
            {
                return;
            }

            if (entityType == "entity_state_player_t")
            {
                UInt32? sequence = (UInt32?)delta.FindEntryValue("sequence");

                if (sequence != null)
                {
                    ConvertSequenceNumber(gameVersion, ref sequence);
                    delta.SetEntryValue("sequence", sequence);
                }

                UInt32? gaitSequence = (UInt32?)delta.FindEntryValue("gaitsequence");

                if (gaitSequence != null)
                {
                    ConvertSequenceNumber(gameVersion, ref gaitSequence);
                    delta.SetEntryValue("gaitsequence", gaitSequence);
                }
            }

            // all entities: zero out animtime
            if (gameVersion == (Int32)Versions.CounterStrike10 || gameVersion == (Int32)Versions.CounterStrike11)
            {
                if (delta.FindEntryValue("animtime") != null)
                {
                    delta.SetEntryValue("animtime", 0.0f);
                }
            }
        }

        public override void ConvertDeltaDescriptionCallback(Int32 gameVersion, String deltaStructureName, HalfLifeDelta delta)
        {
            // CS 1.3 has a different event_t delta description for parameters iparam1 and iparam2.
            // Without this fix, bullets fly off in random directions in HLTV demos. No noticeable affect in POV demos.
            if (gameVersion != (Int32)Versions.CounterStrike13 || deltaStructureName != "event_t" || !((String)delta.FindEntryValue("name")).StartsWith("iparam"))
            {
                return;
            }

            delta.SetEntryValue("nBits", (UInt32)18);
            delta.SetEntryValue("divisor", 1.0f);
        }

        public override void ConvertClCorpseMessageCallback(Int32 gameVersion, BitBuffer bitBuffer)
        {
            if (gameVersion >= (Int32)Versions.CounterStrike16)
            {
                return;
            }

            // Convert the model sequence number.
            // string, 3x long, 3x short, long, byte (sequence number), byte, byte, byte (CS 1.6 only).
            bitBuffer.ReadString(); // model name
            bitBuffer.SeekBytes(22);

            Byte sequence = bitBuffer.ReadByte();
            UInt32? newSequence = sequence;
            ConvertSequenceNumber(gameVersion, ref newSequence);
            
            if (newSequence != sequence)
            {
                // Sequence number has changed, replace the old one with the new one.
                bitBuffer.SeekBytes(-1);
                bitBuffer.RemoveBytes(1);
                bitBuffer.InsertBytes(new Byte[] { (Byte)newSequence });
            }
        }

        private void ConvertSequenceNumber(Int32 gameVersion, ref UInt32? sequence)
        {
            if (gameVersion == (Int32)Versions.CounterStrike16)
            {
                return;
            }

            if (gameVersion == (Int32)Versions.CounterStrike10)
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
        #endregion
    }
}
