using System;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;
using System.IO;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcStuffText : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_stufftext; }
        }

        public override string Name
        {
            get { return "svc_stufftext"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public string Text { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekString();
        }

        public override void Read(BitReader buffer)
        {
            Text = buffer.ReadString();
        }

        public override byte[] Write()
        {
            BitWriter buffer = new BitWriter();
            buffer.WriteString(Text);
            return buffer.ToArray();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Text: \"{0}\"", Text);
        }
    }
}
