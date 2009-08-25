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

        public byte UserMessageId { get; set; }
        public sbyte UserMessageLength { get; set; }
        public string UserMessageName { get; set; }

        public override void Read(BitReader buffer)
        {
            UserMessageId = buffer.ReadByte();
            UserMessageLength = buffer.ReadSByte();
            UserMessageName = buffer.ReadString(16);
        }

        public override byte[] Write()
        {
            BitWriter buffer = new BitWriter();
            buffer.WriteByte(UserMessageId);
            buffer.WriteSByte(UserMessageLength);
            buffer.WriteString(UserMessageName, 16);
            return buffer.Data;
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("ID: {0}", UserMessageId);
            log.WriteLine("Length: {0}", UserMessageLength);
            log.WriteLine("Name: {0}", UserMessageName);
        }
    }
}
