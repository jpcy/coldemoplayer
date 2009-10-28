using System;
using System.IO;

namespace CDP.HalfLife.Messages
{
    public class SvcAddAngle : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_addangle; }
        }

        public override string Name
        {
            get { return "svc_addangle"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public float Angle { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(2);
        }

        public override void Read(BitReader buffer)
        {
            Angle = buffer.ReadHiresAngle();
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteHiresAngle(Angle);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Angle: {0}", Angle);
        }
    }
}
