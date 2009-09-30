using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcTimeScale : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_timescale; }
        }

        public override string Name
        {
            get { return "svc_timescale"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public float Multiplier { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(4);
        }

        public override void Read(BitReader buffer)
        {
            Multiplier = buffer.ReadFloat();
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteFloat(Multiplier);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Multiplier: {0}", Multiplier);
        }
    }
}
