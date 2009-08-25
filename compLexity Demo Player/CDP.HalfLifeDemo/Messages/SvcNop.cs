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

        public override void Read(Core.BitReader buffer)
        {
        }

        public override byte[] Write()
        {
            return null;
        }

#if DEBUG
        public override void Log(StreamWriter log)
        {
        }
#endif
    }
}
