using System;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
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

        public string ClientFallback { get; set; }
        public byte Cheats { get; set; }

        public override void Read(BitReader buffer)
        {
            ClientFallback = buffer.ReadString();
            Cheats = buffer.ReadByte();
        }

        public override byte[] Write()
        {
            BitWriter buffer = new BitWriter();
            buffer.WriteString(ClientFallback);
            buffer.WriteByte(Cheats);
            return buffer.Data;
        }

#if DEBUG
        public override void Log(System.IO.StreamWriter log)
        {
            log.WriteLine("com_clientfallback: {0}", ClientFallback);
            log.WriteLine("sv_cheats: {0}", Cheats);
        }
#endif
    }
}
