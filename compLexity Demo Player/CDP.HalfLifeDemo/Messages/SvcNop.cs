using System;
using System.IO;

namespace CDP.HalfLifeDemo.Messages
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

        public override void Skip(Core.BitReader buffer)
        {
        }

        public override void Read(Core.BitReader buffer)
        {
        }

        public override void Write(Core.BitWriter buffer)
        {
        }

        public override void Log(StreamWriter log)
        {
        }
    }
}
