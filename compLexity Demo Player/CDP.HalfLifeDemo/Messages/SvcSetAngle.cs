using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
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

        public float[] Angle { get; set; }

        public override void Read(BitReader buffer)
        {
            Angle = new float[3];

            for (int i = 0; i < 3; i++)
            {
                Angle[i] = (float)(buffer.ReadShort() * (360.0f / ((1 << 16) - 1)));
            }
        }

        public override byte[] Write()
        {
            BitWriter writer = new BitWriter();

            for (int i = 0; i < 3; i++)
            {
                writer.WriteShort((short)(Angle[i] * (((1 << 16) - 1) / 360.0f)));
            }

            return writer.Data;
        }

#if DEBUG
        public override void Log(StreamWriter log)
        {
            log.WriteLine("Angle: {0}", Angle);
        }
#endif
    }
}
