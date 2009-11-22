using System;
using System.IO;

namespace CDP.HalfLife.Messages
{
    public class SvcSetAngle : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_setangle; }
        }

        public override string Name
        {
            get { return "svc_setangle"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public float[] Angle { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(6);
        }

        public override void Read(BitReader buffer)
        {
            Angle = new float[3];

            for (int i = 0; i < 3; i++)
            {
                Angle[i] = buffer.ReadHiresAngle();
            }
        }

        public override void Write(BitWriter buffer)
        {
            for (int i = 0; i < 3; i++)
            {
                buffer.WriteHiresAngle(Angle[i]);
            }
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Angle: {0}", Angle);
        }
    }
}
