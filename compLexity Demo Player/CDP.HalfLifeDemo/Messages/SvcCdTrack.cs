using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcCdTrack : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_cdtrack; }
        }

        public override string Name
        {
            get { return "svc_cdtrack"; }
        }

        public byte Track { get; set; }
        public byte LoopTrack { get; set; }

        public override void Read(BitReader buffer)
        {
            Track = buffer.ReadByte();
            LoopTrack = buffer.ReadByte();
        }

        public override byte[] Write()
        {
            BitWriter buffer = new BitWriter();
            buffer.WriteByte(Track);
            buffer.WriteByte(LoopTrack);
            return buffer.Data;
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Track: {0}", Track);
            log.WriteLine("Loop track: {0}", LoopTrack);
        }
    }
}
