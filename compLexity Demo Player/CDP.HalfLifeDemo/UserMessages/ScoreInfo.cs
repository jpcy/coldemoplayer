using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.UserMessages
{
    public class ScoreInfo : UserMessage
    {
        public override string Name
        {
            get { return "ScoreInfo"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public byte Slot { get; set; }
        public short Frags { get; set; }
        public short Deaths { get; set; }
        public short Dummy { get; set; }
        public short TeamId { get; set; }

        public override void Read(BitReader buffer)
        {
            Slot = buffer.ReadByte();
            Frags = buffer.ReadShort();
            Deaths = buffer.ReadShort();
            Dummy = buffer.ReadShort();
            TeamId = buffer.ReadShort();
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteByte(Slot);
            buffer.WriteShort(Frags);
            buffer.WriteShort(Deaths);
            buffer.WriteShort(Dummy);
            buffer.WriteShort(TeamId);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Slot: {0}", Slot);
            log.WriteLine("Frags: {0}", Frags);
            log.WriteLine("Deaths: {0}", Deaths);
            log.WriteLine("Dummy: {0}", Dummy);
            log.WriteLine("TeamId: {0}", TeamId);
        }
    }
}
