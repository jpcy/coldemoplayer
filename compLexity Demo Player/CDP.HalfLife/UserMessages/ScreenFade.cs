using System;
using System.IO;

namespace CDP.HalfLife.UserMessages
{
    public class ScreenFade : UserMessage
    {
        public override string Name
        {
            get { return "ScreenFade"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        [Flags]
        public enum FlagBits : ushort
        {
            In = 0,
            Out = (1<<0),
            Modulate = (1<<1),
            StayOut = (1<<2)
        }

        public ushort Duration { get; set; }
        public ushort HoldTime { get; set; }
        public FlagBits Flags { get; set; }
        public uint Colour { get; set; }

        public override void Read(BitReader buffer)
        {
            Duration = buffer.ReadUShort();
            HoldTime = buffer.ReadUShort();
            Flags = (FlagBits)buffer.ReadUShort();
            Colour = buffer.ReadUInt();
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteUShort(Duration);
            buffer.WriteUShort(HoldTime);
            buffer.WriteUShort((ushort)Flags);
            buffer.WriteUInt(Colour);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Duration: {0}", Duration);
            log.WriteLine("Hold time: {0}", HoldTime);
            log.WriteLine("Flags: {0}", Flags);
            log.WriteLine("Colour: {0}", Colour);
        }
    }
}
