using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLife.Messages
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

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        // URL of a HTTP mirror for downloadable resources.
        public string Url { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekString();
        }

        public override void Read(BitReader buffer)
        {
            Url = buffer.ReadString();
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteString(Url);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Url: \"{0}\"", Url);
        }
    }
}
