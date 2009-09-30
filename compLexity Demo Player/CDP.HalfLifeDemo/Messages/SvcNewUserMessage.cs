using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcNewUserMessage : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_newusermsg; }
        }

        public override string Name
        {
            get { return "svc_newusermsg"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public byte UserMessageId { get; set; }
        public sbyte UserMessageLength { get; set; }
        public string UserMessageName { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(18);
        }

        public override void Read(BitReader buffer)
        {
            UserMessageId = buffer.ReadByte();
            UserMessageLength = buffer.ReadSByte();
            UserMessageName = buffer.ReadString(16);
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteByte(UserMessageId);
            buffer.WriteSByte(UserMessageLength);
            buffer.WriteString(UserMessageName, 16);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("ID: {0}", UserMessageId);
            log.WriteLine("Length: {0}", UserMessageLength);
            log.WriteLine("Name: {0}", UserMessageName);
        }
    }
}
