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

        public float Multiplier { get; set; }

        public override void Read(BitReader buffer)
        {
            Multiplier = buffer.ReadFloat();
        }

        public override byte[] Write()
        {
            throw new NotImplementedException();
        }

#if DEBUG
        public override void Log(StreamWriter log)
        {
            log.WriteLine("Multiplier: {0}", Multiplier);
        }
#endif
    }
}
