using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcTime : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_time; }
        }

        public override string Name
        {
            get { return "svc_time"; }
        }

        public float Timestamp { get; set; }

        public override void Read(BitReader buffer)
        {
            Timestamp = buffer.ReadFloat();
        }

        public override byte[] Write()
        {
            BitWriter buffer = new BitWriter();
            buffer.WriteFloat(Timestamp);
            return buffer.Data;
        }

#if DEBUG
        public override void Log(StreamWriter log)
        {
            log.WriteLine("Timestamp: {0}", Timestamp);
        }
#endif
    }
}
