using System;
using System.IO;
using CDP.Core;

namespace CDP.Source.Messages
{
    public class SvcPrint : Message
    {
        public override MessageIds Id
        {
            get { return MessageIds.SVC_Print; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.SVC_Print; }
        }

        public override string Name
        {
            get { return "SVC_Print"; }
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
