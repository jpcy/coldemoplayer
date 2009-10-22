using System;
using System.IO;
using CDP.Core;

namespace CDP.Source.Messages
{
    public class SvcVoiceData : Message
    {
        public override MessageIds Id
        {
            get { return MessageIds.SVC_VoiceData; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.SVC_VoiceData; }
        }

        public byte Client { get; set; }
        public byte Unknown1 { get; set; }
        public ushort NumBits { get; set; }
        public bool Unknown2 { get; set; }
        public bool Unknown3 { get; set; }
        public bool Unknown4 { get; set; }
        public bool Unknown5 { get; set; }

        public override string Name
        {
            get { return "SVC_VoiceData"; }
        }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(1);

            if (Demo.NetworkProtocol >= 14)
            {
                buffer.SeekBytes(1);
            }

            ushort nBits = buffer.ReadUShort();

            if (Demo.NetworkProtocol >= 36)
            {
                buffer.SeekBits(4);
            }

            buffer.SeekBits(nBits);
        }

        public override void Read(BitReader buffer)
        {
            Client = buffer.ReadByte();

            if (Demo.NetworkProtocol >= 14)
            {
                Unknown1 = buffer.ReadByte();
            }

            NumBits = buffer.ReadUShort();

            if (Demo.NetworkProtocol >= 36)
            {
                Unknown2 = buffer.ReadBoolean();
                Unknown3 = buffer.ReadBoolean();
                Unknown4 = buffer.ReadBoolean();
                Unknown5 = buffer.ReadBoolean();
            }

            buffer.SeekBits(NumBits);
        }

        public override void Write(BitWriter buffer)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Client: {0}", Client);

            if (Demo.NetworkProtocol >= 15)
            {
                log.WriteLine("Unknown1: {0}", Unknown1);
            }

            log.WriteLine("Num. bits: {0}", NumBits);

            if (Demo.NetworkProtocol >= 36)
            {
                log.WriteLine("Unknown2: {0}", Unknown2);
                log.WriteLine("Unknown3: {0}", Unknown3);
                log.WriteLine("Unknown4: {0}", Unknown4);
                log.WriteLine("Unknown5: {0}", Unknown5);
            }
        }
    }
}
