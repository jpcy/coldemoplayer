using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcResourceLocation : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_resourcelocation; }
        }

        public override string Name
        {
            get { return "svc_resourcelocation"; }
        }

        // URL of a HTTP mirror for downloadable resources?
        public string Location { get; set; }

        public override void Read(BitReader buffer)
        {
            Location = buffer.ReadString();
        }

        public override byte[] Write()
        {
            throw new NotImplementedException();
        }

#if DEBUG
        public override void Log(StreamWriter log)
        {
            throw new NotImplementedException();
        }
#endif
    }
}
