using System;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcResourceRequest : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_resourcerequest; }
        }

        public override string Name
        {
            get { return "svc_resourcerequest"; }
        }

        public uint ServerProcessCount { get; set; }
        public uint Unknown { get; set; }

        public override void Read(BitReader buffer)
        {
            ServerProcessCount = buffer.ReadUInt();
            Unknown = buffer.ReadUInt();
        }

        public override byte[] Write()
        {
            BitWriter buffer = new BitWriter();
            buffer.WriteUInt(ServerProcessCount);
            buffer.WriteUInt(Unknown);
            return buffer.Data;
        }

#if DEBUG
        public override void Log(System.IO.StreamWriter log)
        {
            log.WriteLine("Server process count: {0}", ServerProcessCount);
            log.WriteLine("Unknown: {0}", Unknown);
        }
#endif
    }
}
