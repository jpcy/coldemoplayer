using System;
using System.IO;

namespace CDP.HalfLife.Messages
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

        public override bool CanSkipWhenWriting
        {
            get { return demo.NetworkProtocol >= 47; } // TODO: beta steam check
        }

        public string Codec { get; set; }
        public byte Quality { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekString();

            if (demo.NetworkProtocol >= 46 && buffer.BytesLeft > 0)
            {
                byte quality = buffer.ReadByte();

                if (quality < 1 || quality > 5)
                {
                    buffer.SeekBytes(-1);
                }
            }
        }

        public override void Read(BitReader buffer)
        {
            Codec = buffer.ReadString();
            Quality = 5;

            if (demo.NetworkProtocol >= 46 && buffer.BytesLeft > 0)
            {
                byte quality = buffer.ReadByte();

                // In the transition of the Half-Life engine to Steam, Valve made changes to the network protocol without incrementing the actual network protocol number. Before Steam there was no quality byte. This is the only non-mod-specific way of handling this.
                if (quality < 1 || quality > 5)
                {
                    buffer.SeekBytes(-1);
                }
                else
                {
                    Quality = quality;
                }
            }
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteString(Codec);
            buffer.WriteByte(Quality);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Codec: {0}", Codec);
            log.WriteLine("Quality: {0}", Quality);
        }
    }
}
