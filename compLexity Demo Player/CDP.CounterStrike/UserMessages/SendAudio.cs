using System;
using System.IO;

namespace CDP.CounterStrike.UserMessages
{
    public class SendAudio : HalfLife.UserMessage
    {
        public override string Name
        {
            get { return "SendAudio"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return false; }
        }

        public byte Slot { get; set; }
        public string SoundName { get; set; }
        public short? Pitch { get; set; }

        public override void Read(HalfLife.BitReader buffer)
        {
            int startOffset = buffer.CurrentByte;
            Slot = buffer.ReadByte();
            SoundName = buffer.ReadString();

            if (buffer.CurrentByte - startOffset < Length)
            {
                Pitch = buffer.ReadShort();
            }
            else
            {
                // Written message will have an extra 2 bytes for pitch.
                Length += 2;
            }
        }

        public override void Write(HalfLife.BitWriter buffer)
        {
            buffer.WriteByte(Slot);
            buffer.WriteString(SoundName);

            if (Pitch.HasValue)
            {
                buffer.WriteShort(Pitch.Value);
            }
            else
            {
                buffer.WriteShort(100);
            }
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Slot: {0}", Slot);
            log.WriteLine("Sound: {0}", SoundName);
            log.WriteLine("Pitch: {0}", Pitch);
        }
    }
}
