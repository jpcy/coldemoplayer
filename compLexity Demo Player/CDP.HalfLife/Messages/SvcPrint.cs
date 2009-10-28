using System;
using System.IO;

namespace CDP.HalfLife.Messages
{
    public class SvcPrint : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_print; }
        }

        public override string Name
        {
            get { return "svc_print"; }
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

        public override void Write(BitWriter buffer)
        {
            buffer.WriteString(Text);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine(Text);
        }
    }
}
