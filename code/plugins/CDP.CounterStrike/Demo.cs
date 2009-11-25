using System;
using System.Linq;
using System.Collections.Generic;
using CDP.HalfLife.Messages;
using CDP.HalfLife.UserMessages;

namespace CDP.CounterStrike
{
    public class Demo : HalfLife.Demo
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

        public Versions Version { get; private set; }

        public override string MapThumbnailRelativePath
        {
            get
            {
                return fileSystem.PathCombine("goldsrc", "cstrike", MapName + ".jpg");
            }
        }

        public override bool CanPlay
        {
            get { return true; }
        }

        public override bool CanAnalyse
        {
            get { return true; }
        }

        /// <summary>
        ///  Old user messages that don't exist in the current version of Counter-Strike.
        /// </summary>
        private Dictionary<string, byte> extraUserMessages = new Dictionary<string, byte>();

        /// <summary>
        /// The resource index of the createsmoke event.
        /// </summary>
        private uint? createSmokeEventIndex;

        public override void Load()
        {
            base.Load();
            Version = (Versions)Game.GetVersion(clientDllChecksum);
        }

        public override void Write(string destinationFileName)
        {
            // Clear state (Write may be called on the same demo multiple times).
            extraUserMessages.Clear();
            createSmokeEventIndex = null;

            // Add message handlers.
            // Doing as many version checks as possible here rather than in the handlers themselves is faster since messages without handlers can often be skipped.
            if (Version == Versions.CounterStrike13)
            {
                AddMessageCallback<SvcDeltaDescription>(Write_DeltaDescription);
            }

            AddMessageCallback<SvcNewUserMessage>(Write_NewUserMessage);

            if (Version != Versions.CounterStrike16)
            {
                AddMessageCallback<SvcResourceList>(Write_ResourceList);
                AddMessageCallback<SvcSpawnBaseline>(Write_SpawnBaseline);
                AddMessageCallback<SvcPacketEntities>(Write_PacketEntities);
                AddMessageCallback<SvcDeltaPacketEntities>(Write_DeltaPacketEntities);
                AddMessageCallback<UserMessages.ClCorpse>(Write_ClCorpse);
            }

            if (Version == Versions.CounterStrike10 || Version == Versions.CounterStrike11 || Version == Versions.CounterStrike13)
            {
                AddMessageCallback<SvcEvent>(Write_Event);
            }

            AddMessageCallback<ScreenFade>(Write_ScreenFade);

            // Start writing.
            base.Write(destinationFileName);
        }

        /// <summary>
        /// Gets an ID for a given user message name.
        /// </summary>
        /// <param name="userMessageName">The user message name.</param>
        /// <returns>The user message's ID.</returns>
        protected override byte GetUserMessageId(string userMessageName)
        {
            CounterStrike.Game.UserMessage[] userMessages = ((CounterStrike.Game)Game).UserMessages;
            CounterStrike.Game.UserMessage userMessage = userMessages.FirstOrDefault(um => um.Name == userMessageName);

            if (userMessage == null)
            {
                if (extraUserMessages.ContainsKey(userMessageName))
                {
                    return extraUserMessages[userMessageName];
                }

                // Find the the first free user message ID.
                for (byte i = (byte)(Demo.MaxEngineMessageId + 1); i < 255; i++)
                {
                    if (userMessages.FirstOrDefault(um => um.Id == i) == null && !extraUserMessages.ContainsValue(i))
                    {
                        extraUserMessages.Add(userMessageName, i);
                        return i;
                    }
                }

                throw new ApplicationException("No free user message indices.");
            }

            return userMessage.Id;
        }

        private void Write_DeltaDescription(SvcDeltaDescription message)
        {
            // CS 1.3 has a different event_t delta description for parameters iparam1 and iparam2.
            // Without this fix, bullets fly off in random directions in HLTV demos. No noticeable affect in POV demos.
            if (message.Structure.Name == "event_t")
            {
                // FIXME: for writing only (see SvcDeltaDescription class).
                foreach (var delta in message.Deltas.Where(d => ((string)d.FindEntryValue("name")).StartsWith("iparam")))
                {
                    delta.SetEntryValue("nBits", (uint)18);
                    delta.SetEntryValue("divisor", 1.0f);
                }

                // Change the actual structure, which will be used for writing deltas from now on.
                for (int i = 1; i <= 2; i++)
                {
                    message.Structure.AddEntry(string.Format("iparam{0}", i), 18, 1.0f, CDP.HalfLife.DeltaStructure.EntryFlags.Integer | CDP.HalfLife.DeltaStructure.EntryFlags.Signed);
                }

                // Overwrite the existing structure.
                AddWriteDeltaStructure(message.Structure);
            }
        }

        private void Write_NewUserMessage(SvcNewUserMessage message)
        {
            message.UserMessageId = GetUserMessageId(message.UserMessageName);
        }

        private void Write_ResourceList(SvcResourceList message)
        {
            CounterStrike.Game.Resource[] blacklist = ((CounterStrike.Game)Game).ResourceBlacklist;
            message.Resources.RemoveAll(r => blacklist.FirstOrDefault(blr => r.Name == blr.Name) != null);

            foreach (SvcResourceList.Resource resource in message.Resources)
            {
                // Store createsmoke event ID.
                if (resource.Type == SvcResourceList.Resource.Types.EventScript && resource.Name == "events/createsmoke.sc")
                {
                    createSmokeEventIndex = resource.Index;
                }

                // Old CS versions use seperate models for left and right handed weapon models.
                if (resource.Name.EndsWith("_r.mdl"))
                {
                    resource.Name = resource.Name.Replace("_r.mdl", ".mdl");
                }
            }
        }

        private void Write_SpawnBaseline(SvcSpawnBaseline message)
        {
            foreach (var entity in message.Entities)
            {
                if (entity.TypeName == "entity_state_player_t")
                {
                    ConvertPlayerDelta(entity.Delta);
                }

                ConvertEntityDelta(entity.Delta);
            }
        }

        private void Write_PacketEntities(SvcPacketEntities message)
        {
            foreach (var entity in message.Entities)
            {
                if (entity.TypeName == "entity_state_player_t")
                {
                    ConvertPlayerDelta(entity.Delta);
                }

                ConvertEntityDelta(entity.Delta);
            }
        }

        private void Write_DeltaPacketEntities(SvcDeltaPacketEntities message)
        {
            foreach (var entity in message.Entities.Where(e => e.Delta != null))
            {
                if (entity.TypeName == "entity_state_player_t")
                {
                    ConvertPlayerDelta(entity.Delta);
                }

                ConvertEntityDelta(entity.Delta);
            }
        }

        private void Write_Event(SvcEvent message)
        {
            // Smoke fix for CS 1.0-1.1 demos.
            if ((Version == Versions.CounterStrike10 || Version == Versions.CounterStrike11) && createSmokeEventIndex.HasValue)
            {
                foreach (SvcEvent.Event createSmokeEvent in message.Events.Where(ev => ev.Index == createSmokeEventIndex))
                {
                    if (createSmokeEvent.Delta.FindEntryValue("iparam1") != null)
                    {
                        Random r = new Random();
                        createSmokeEvent.Delta.SetEntryValue("iparam1", r.Next(128, 300)); // FIXME: random guess.
                    }
                }
            }

            // iparam fix for CS 1.3 demos.
            if (Version == Versions.CounterStrike13)
            {
                foreach (SvcEvent.Event ev in message.Events.Where(e => e.Delta != null))
                {
                    for (int i = 1; i <= 2; i++)
                    {
                        string paramName = "iparam" + i.ToString();
                        int? value = (int?)ev.Delta.FindEntryValue(paramName);

                        if (value != null)
                        {
                            ev.Delta.SetEntryValue(paramName, value / 8192);
                        }
                    }
                }
            }
        }

        private void Write_ClCorpse(UserMessages.ClCorpse message)
        {
            // Client-side player corpse.
            message.Sequence = (byte)ConvertPlayerSequenceNumber(message.Sequence);
        }

        private void Write_ScreenFade(ScreenFade message)
        {
            // Remove fade to black.
            if (Perspective == "POV" && (bool)settings["CsRemoveFadeToBlack"] && message.Duration == 0x3000 && message.HoldTime == 0x3000 && message.Flags == (ScreenFade.FlagBits.Out | ScreenFade.FlagBits.StayOut) && message.Colour == 0xFF000000)
            {
                message.Remove = true;
            }
        }

        /// <summary>
        /// Converts a CS 1.0-1.5 entity delta to 1.6.
        /// </summary>
        /// <param name="delta">The entity delta.</param>
        private void ConvertEntityDelta(CDP.HalfLife.Delta delta)
        {
            if (Version == Versions.CounterStrike10 || Version == Versions.CounterStrike11)
            {
                if (delta.FindEntryValue("animtime") != null)
                {
                    delta.SetEntryValue("animtime", 0.0f);
                }
            }
        }

        /// <summary>
        /// Converts a CS 1.0-1.5 player delta's animation sequences to 1.6.
        /// </summary>
        /// <param name="delta">The player delta.</param>
        private void ConvertPlayerDelta(CDP.HalfLife.Delta delta)
        {
            uint? sequence = (uint?)delta.FindEntryValue("sequence");

            if (sequence.HasValue)
            {
                delta.SetEntryValue("sequence", ConvertPlayerSequenceNumber(sequence.Value));
            }

            uint? gaitSequence = (uint?)delta.FindEntryValue("gaitsequence");

            if (gaitSequence.HasValue)
            {
                delta.SetEntryValue("gaitsequence", ConvertPlayerSequenceNumber(gaitSequence.Value));
            }
        }

        /// <summary>
        /// Converts a CS 1.0-1.5 player model sequence or gaitsequence to match 1.6.
        /// </summary>
        /// <param name="sequence">The sequence or gaitsequence number to convert.</param>
        /// <returns>The converted sequence or gaitsequence number.</returns>
        private uint ConvertPlayerSequenceNumber(uint sequence)
        {
            if (Version ==Versions.CounterStrike10)
            {
                if (sequence <= 6) // 0 to 6 map to 1 to 7
                {
                    return sequence + 1; // no dummy in CS 1.0
                }
                else if (sequence >= 7 && sequence <= 79) // 7+ maps to 10+ until 79
                {
                    return sequence + 3; // no dummy (as above) and no swim or treadwater sequences
                }
                else
                {
                    return sequence + 19; // no shield sequences
                }
            }
            else // 1.1-1.5
            {
                if (sequence >= 83)
                {
                    return sequence + 16;
                }
            }

            return sequence;
        }
    }
}
