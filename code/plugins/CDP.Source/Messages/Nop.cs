using System;
using System.IO;
using CDP.Core;

namespace CDP.Source.Messages
{
    public class Nop : Message
    {
        public override MessageIds Id
        {
            get { return MessageIds.Nop; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.Nop; }
        }

        public override string Name
        {
            get { return "Nop"; }
        }

        public override void Skip(BitReader buffer)
        {
        }

        public override void Read(BitReader buffer)
        {
        }

        public override void Write(BitWriter buffer)
        {
        }

        public override void Log(StreamWriter log)
        {
        }
    }
}
