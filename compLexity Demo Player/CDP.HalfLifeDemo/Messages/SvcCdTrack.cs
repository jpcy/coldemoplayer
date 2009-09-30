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

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public byte Track { get; set; }
        public byte LoopTrack { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(2);
        }

        public override void Read(BitReader buffer)
        {
            Track = buffer.ReadByte();
            LoopTrack = buffer.ReadByte();
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteByte(Track);
            buffer.WriteByte(LoopTrack);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Track: {0}", Track);
            log.WriteLine("Loop track: {0}", LoopTrack);
        }
    }
}
