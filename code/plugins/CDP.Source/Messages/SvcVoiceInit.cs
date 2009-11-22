using System;
using System.IO;
using CDP.Core;

namespace CDP.Source.Messages
{
    public class SvcVoiceInit : Message
    {
        public override MessageIds Id
        {
            get { return MessageIds.SVC_VoiceInit; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.SVC_VoiceInit; }
        }

        public override string Name
        {
            get { return "SVC_VoiceInit"; }
        }

        public string Codec { get; set; }
        public byte Quality { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekString();
            buffer.SeekBytes(1);
        }

        public override void Read(BitReader buffer)
        {
            Codec = buffer.ReadString();
            Quality = buffer.ReadByte();
        }

        public override void Write(BitWriter buffer)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Codec: {0}", Codec);
            log.WriteLine("Quality: {0}", Quality);
        }
    }
}
