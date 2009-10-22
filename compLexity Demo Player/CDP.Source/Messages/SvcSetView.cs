using System;
using System.IO;
using CDP.Core;

namespace CDP.Source.Messages
{
    public class SvcSetView : Message
    {
        public override MessageIds Id
        {
            get { return MessageIds.SVC_SetView; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.SVC_SetView; }
        }

        public override string Name
        {
            get { return "SVC_SetView"; }
        }

        public uint ViewEntityNumber { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBits(11);
        }

        public override void Read(BitReader buffer)
        {
            ViewEntityNumber = buffer.ReadUBits(11);
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteUBits(ViewEntityNumber, 11);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("View entity number: {0}", ViewEntityNumber);
        }
    }
}
