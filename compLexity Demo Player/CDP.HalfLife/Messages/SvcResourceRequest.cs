using System;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLife.Messages
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

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public uint ServerProcessCount { get; set; }
        public uint Unknown { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(8);
        }

        public override void Read(BitReader buffer)
        {
            ServerProcessCount = buffer.ReadUInt();
            Unknown = buffer.ReadUInt();
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteUInt(ServerProcessCount);
            buffer.WriteUInt(Unknown);
        }

        public override void Log(System.IO.StreamWriter log)
        {
            log.WriteLine("Server process count: {0}", ServerProcessCount);
            log.WriteLine("Unknown: {0}", Unknown);
        }
    }
}
