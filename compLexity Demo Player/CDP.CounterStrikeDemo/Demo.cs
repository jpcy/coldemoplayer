using System;
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

        public override void Load()
        {
            base.Load();

            if (Game != null)
            {
                Version = (Versions)Game.GetVersion(clientDllChecksum);
            }
        }

        public override void Write(string destinationFileName)
        {
            AddMessageCallback<ScreenFade>(Write_ScreenFade);
            base.Write(destinationFileName);
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
