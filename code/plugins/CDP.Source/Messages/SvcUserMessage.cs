using System;
using System.IO;
using CDP.Core;

namespace CDP.Source.Messages
{
    public class SvcUserMessage : Message
    {
        public override MessageIds Id
        {
            get { return MessageIds.SVC_UserMessage; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.SVC_UserMessage; }
        }

        public override string Name
        {
            get { return "SVC_UserMessage"; }
        }

        public byte UserMessageId { get; set; }
        public uint NumBits { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(1);
            uint nBits = buffer.ReadUBits(11);
            buffer.SeekBits(nBits);
        }

        public override void Read(BitReader buffer)
        {
            UserMessageId = buffer.ReadByte();
            NumBits = buffer.ReadUBits(11);
            buffer.SeekBits(NumBits);
        }

        public override void Write(BitWriter buffer)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("User message ID: {0}", UserMessageId);
            log.WriteLine("Num. bits: {0}", NumBits);
        }
    }
}
