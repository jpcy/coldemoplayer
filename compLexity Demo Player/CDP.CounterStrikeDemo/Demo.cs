using System;
using System.Linq;
using System.Collections.Generic;
using CDP.HalfLifeDemo.Messages;
using CDP.HalfLifeDemo.UserMessages;

namespace CDP.CounterStrikeDemo
{
    public class Demo : HalfLifeDemo.Demo
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

        public override string MapImagesRelativePath
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

        // Old user messages that don't exist in the current version of Counter-Strike.
        private Dictionary<string, byte> extraUserMessages = new Dictionary<string, byte>();

        public override void Load()
        {
            base.Load();
            Version = (Versions)Game.GetVersion(clientDllChecksum);
        }

        public override void Write(string destinationFileName)
        {
            AddMessageCallback<SvcNewUserMessage>(Write_NewUserMessage);

            if (Version != Versions.CounterStrike16)
            {
                AddMessageCallback<SvcResourceList>(Write_ResourceList);
            }

            AddMessageCallback<ScreenFade>(Write_ScreenFade);
            base.Write(destinationFileName);
        }

        protected override byte GetUserMessageId(string userMessageName)
        {
            CounterStrikeDemo.Game.UserMessage[] userMessages = ((CounterStrikeDemo.Game)Game).UserMessages;
            CounterStrikeDemo.Game.UserMessage userMessage = userMessages.FirstOrDefault(um => um.Name == userMessageName);

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

        private void Write_NewUserMessage(SvcNewUserMessage message)
        {
            message.UserMessageId = GetUserMessageId(message.UserMessageName);
        }

        private void Write_ResourceList(SvcResourceList message)
        {
            CounterStrikeDemo.Game.Resource[] blacklist = ((CounterStrikeDemo.Game)Game).ResourceBlacklist;
            message.Resources.RemoveAll(r => blacklist.FirstOrDefault(blr => r.Name == blr.Name) != null);

            foreach (SvcResourceList.Resource resource in message.Resources)
            {
                // Old CS versions use seperate models for left and right handed weapon models.
                if (resource.Name.EndsWith("_r.mdl"))
                {
                    resource.Name = resource.Name.Replace("_r.mdl", ".mdl");
                }
            }
        }

        private void Write_ScreenFade(ScreenFade message)
        {
            if (Perspective == "POV" && (bool)settings["CsRemoveFadeToBlack"] && message.Duration == 0x3000 && message.HoldTime == 0x3000 && message.Flags == (ScreenFade.FlagBits.Out | ScreenFade.FlagBits.StayOut) && message.Colour == 0xFF000000)
            {
                message.Remove = true;
            }
        }
    }
}
