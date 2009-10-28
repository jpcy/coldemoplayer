using System;
using System.IO;

namespace CDP.HalfLife.Messages
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

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public float Timestamp { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(4);
        }

        public override void Read(BitReader buffer)
        {
            Timestamp = buffer.ReadFloat();
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteFloat(Timestamp);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Timestamp: {0}", Timestamp);
        }
    }
}
