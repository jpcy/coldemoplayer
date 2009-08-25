using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
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

        public string Text { get; set; }

        public override void Read(BitReader buffer)
        {
            Text = buffer.ReadString();
        }

        public override byte[] Write()
        {
            BitWriter buffer = new BitWriter();
            buffer.WriteString(Text);
            return buffer.Data;
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine(Text);
        }
    }
}
