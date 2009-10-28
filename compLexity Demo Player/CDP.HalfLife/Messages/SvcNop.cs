using System;
using System.IO;

namespace CDP.HalfLife.Messages
{
    public class SvcNop : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_nop; }
        }

        public override string Name
        {
            get { return "svc_nop"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public override void Skip(BitReader buffer)
        {
        }

        public override void Read(BitReader buffer)
        {
        }

        public override void Write(BitWriter buffer)
        {
        }

        public override void Log(StreamWriter log)
        {
        }
    }
}
