using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcDirector : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_director; }
        }

        public override string Name
        {
            get { return "svc_director"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public byte[] Data { get; set; }

        public override void Skip(BitReader buffer)
        {
            byte length = buffer.ReadByte();
            buffer.SeekBytes(length);
        }

        public override void Read(BitReader buffer)
        {
            byte length = buffer.ReadByte();

            if (length > 0)
            {
                Data = buffer.ReadBytes(length);
            }
        }

        public override void Write(BitWriter buffer)
        {
            if (Data == null)
            {
                buffer.WriteByte(0);
            }
            else
            {
                buffer.WriteByte((byte)Data.Length);
                buffer.WriteBytes(Data);
            }
        }

        public override void Log(StreamWriter log)
        {
            if (Data != null)
            {
                log.WriteLine("Length: {0}", Data.Length);
            }
        }
    }
}
