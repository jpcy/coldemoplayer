using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcSignOnNum : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_signonnum; }
        }

        public override string Name
        {
            get { return "svc_signonnum"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public byte Number { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(1);
        }

        public override void Read(BitReader buffer)
        {
            Number = buffer.ReadByte(); // Always 1.
        }

        public override byte[] Write()
        {
            return new byte[] { Number };
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Number: {0}", Number);
        }
    }
}
