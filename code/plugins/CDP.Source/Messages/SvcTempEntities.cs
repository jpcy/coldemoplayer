using System;
using System.IO;
using System.Collections.Generic;
using CDP.Core;

namespace CDP.Source.Messages
{
    public class SvcTempEntities : Message
    {
        public override MessageIds Id
        {
            get { return MessageIds.SVC_TempEntities; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.SVC_TempEntities; }
        }

        public override string Name
        {
            get { return "SVC_TempEntities"; }
        }

        public byte Count { get; set; }
        public uint NumBits { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(1);
            uint nBits = buffer.ReadUBits(17);
            buffer.SeekBits(nBits);
        }

        public override void Read(BitReader buffer)
        {
            Count = buffer.ReadByte();
            NumBits = buffer.ReadUBits(17);
            buffer.SeekBits(NumBits);
        }

        public override void Write(BitWriter buffer)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Count: {0}", Count);
            log.WriteLine("Num. bits: {0}", NumBits);
        }
    }
}
