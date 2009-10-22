using System;
using System.IO;
using System.Collections.Generic;
using CDP.Core;

namespace CDP.Source.Messages
{
    public class SvcGameEvent : Message
    {
        public override MessageIds Id
        {
            get { return MessageIds.SVC_GameEvent; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.SVC_GameEvent; }
        }

        public override string Name
        {
            get { return "SVC_GameEvent"; }
        }

        public uint NumBits { get; set; }
        public uint GameEventId { get; set; }

        public override void Skip(BitReader buffer)
        {
            uint nBits = buffer.ReadUBits(11);
            buffer.SeekBits(nBits);
        }

        public override void Read(BitReader buffer)
        {
            NumBits = buffer.ReadUBits(11);
            GameEventId = buffer.ReadUBits(9);
            buffer.SeekBits(NumBits - 9);
        }

        public override void Write(BitWriter buffer)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Num. bits: {0}", NumBits);
            log.WriteLine("Game event ID: {0}", GameEventId);
        }
    }
}
