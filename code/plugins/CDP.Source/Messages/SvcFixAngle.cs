using System;
using System.IO;
using CDP.Core;
using CDP.Core.Extensions;

namespace CDP.Source.Messages
{
    public class SvcFixAngle : Message
    {
        public override MessageIds Id
        {
            get { return MessageIds.SVC_FixAngle; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.SVC_FixAngle; }
        }

        public override string Name
        {
            get { return "SVC_FixAngle"; }
        }

        public Vector Angle { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBits(48);
        }

        public override void Read(BitReader buffer)
        {
            Angle = new Vector();
            Angle.X = buffer.ReadBitAngle(16) / 2.0f;
            Angle.Y = buffer.ReadBitAngle(16) / 2.0f;
            Angle.Z = buffer.ReadBitAngle(16) / 2.0f;
        }

        public override void Write(BitWriter buffer)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteVector("Angle", Angle);
        }
    }
}
