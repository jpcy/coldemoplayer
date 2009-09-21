using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcVoiceData : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_voicedata; }
        }

        public override string Name
        {
            get { return "svc_voicedata"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public byte Slot { get; set; }
        public byte[] Data { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(1);
            ushort length = buffer.ReadUShort();
            buffer.SeekBytes(length);
        }

        public override void Read(BitReader buffer)
        {
            Slot = buffer.ReadByte();
            ushort length = buffer.ReadUShort();
            Data = buffer.ReadBytes(length);
        }

        public override byte[] Write()
        {
            BitWriter buffer = new BitWriter();
            buffer.WriteByte(Slot);

            if (Data == null)
            {
                buffer.WriteUShort(0);
            }
            else
            {
                buffer.WriteUShort((ushort)Data.Length);
                buffer.WriteBytes(Data);
            }

            return buffer.ToArray();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Slot: {0}", Slot);

            if (Data != null)
            {
                log.WriteLine("Length: {0}", Data.Length);
            }
        }
    }
}
