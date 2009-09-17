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

        public byte Slot { get; set; }
        public short Frags { get; set; }
        public short Deaths { get; set; }
        public short TeamId { get; set; }

        public override void Read(BitReader buffer)
        {
            Slot = buffer.ReadByte();
            Frags = buffer.ReadShort();
            Deaths = buffer.ReadShort();
            buffer.SeekBytes(2); // Should be 0 (short).
            TeamId = buffer.ReadShort();
        }

        public override byte[] Write()
        {
            throw new NotImplementedException();
        }

#if DEBUG
        public override void Log(StreamWriter log)
        {
            throw new NotImplementedException();
        }
#endif
    }
}
