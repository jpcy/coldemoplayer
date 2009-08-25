using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcVoiceInit : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_voiceinit; }
        }

        public override string Name
        {
            get { return "svc_voiceinit"; }
        }

        public string Codec { get; set; }
        public byte Quality { get; set; }

        public override void Read(BitReader buffer)
        {
            Codec = buffer.ReadString();

            if (demo.NetworkProtocol >= 47) // TODO: beta steam check
            {
                Quality = buffer.ReadByte();
            }
            else
            {
                Quality = 5;
            }
        }

        public override byte[] Write()
        {
            BitWriter buffer = new BitWriter();
            buffer.WriteString(Codec);
            buffer.WriteByte(Quality);
            return buffer.Data;
        }

#if DEBUG
        public override void Log(StreamWriter log)
        {
            log.WriteLine("Codec: {0}", Codec);
            log.WriteLine("Quality: {0}", Quality);
        }
#endif
    }
}
