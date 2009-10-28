using System;

namespace CDP.HalfLife.Messages
{
    public class SvcSendExtraInfo : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_sendextrainfo; }
        }

        public override string Name
        {
            get { return "svc_sendextrainfo"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public string ClientFallback { get; set; }
        public byte Cheats { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekString();
            buffer.SeekBytes(1);
        }

        public override void Read(BitReader buffer)
        {
            ClientFallback = buffer.ReadString();
            Cheats = buffer.ReadByte();
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteString(ClientFallback);
            buffer.WriteByte(Cheats);
        }

        public override void Log(System.IO.StreamWriter log)
        {
            log.WriteLine("com_clientfallback: {0}", ClientFallback);
            log.WriteLine("sv_cheats: {0}", Cheats);
        }
    }
}
