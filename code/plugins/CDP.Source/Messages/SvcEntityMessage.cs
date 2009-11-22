using System;
using System.IO;
using CDP.Core;

namespace CDP.Source.Messages
{
    public class SvcEntityMessage : Message
    {
        public override MessageIds Id
        {
            get { return MessageIds.SVC_EntityMessage; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.SVC_EntityMessage; }
        }

        public override string Name
        {
            get { return "SVC_EntityMessage"; }
        }

        public uint Unknown1 { get; set; }
        public uint Unknown2 { get; set; }
        public uint NumBits { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBits(20);
            uint nBits = buffer.ReadUBits(11);
            buffer.SeekBits(nBits);
        }

        public override void Read(BitReader buffer)
        {
            Unknown1 = buffer.ReadUBits(11);
            Unknown2 = buffer.ReadUBits(9);
            NumBits = buffer.ReadUBits(11);
            buffer.SeekBits(NumBits);
        }

        public override void Write(BitWriter buffer)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Unknown1: {0}", Unknown1);
            log.WriteLine("Unknown2: {0}", Unknown2);
            log.WriteLine("Num. bits: {0}", NumBits);
        }
    }
}
