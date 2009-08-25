using System;
using System.IO;
using System.Collections.Generic;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcPings : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_pings; }
        }

        public override string Name
        {
            get { return "svc_pings"; }
        }

        public class Client
        {
            public uint Slot { get; set; }
            public uint Ping { get; set; }
            public uint Loss { get; set; }
        }

        public List<Client> Clients { get; set; }

        public override void Read(BitReader buffer)
        {
            if (demo.NetworkProtocol <= 43)
            {
                buffer.Endian = BitReader.Endians.Big;
            }

            Clients = new List<Client>();

            while (buffer.ReadBoolean())
            {
                Clients.Add(new Client
                {
                    Slot = buffer.ReadUnsignedBits(5),
                    Ping = buffer.ReadUnsignedBits(12),
                    Loss = buffer.ReadUnsignedBits(7)
                });
            }

            buffer.SkipRemainingBitsInCurrentByte();
            buffer.Endian = BitReader.Endians.Little;
        }

        public override byte[] Write()
        {
            BitWriter buffer = new BitWriter();

            foreach (Client client in Clients)
            {
                buffer.WriteBoolean(true);
                buffer.WriteUnsignedBits(client.Slot, 5);
                buffer.WriteUnsignedBits(client.Ping, 12);
                buffer.WriteUnsignedBits(client.Loss, 7);
            }

            buffer.WriteBoolean(false);
            return buffer.Data;
        }

#if DEBUG
        public override void Log(StreamWriter log)
        {
            log.WriteLine("Num clients: {0}", Clients.Count);
            // TODO
        }
#endif
    }
}
