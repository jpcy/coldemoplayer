using System;
using System.IO;
using System.Collections.Generic;

namespace CDP.HalfLife.Messages
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

        public override bool CanSkipWhenWriting
        {
            get { return demo.NetworkProtocol > 43; }
        }

        public class Client
        {
            public uint Slot { get; set; }
            public uint Ping { get; set; }
            public uint Loss { get; set; }
        }

        public List<Client> Clients { get; set; }

        public override void Skip(BitReader buffer)
        {
            if (demo.NetworkProtocol <= 43)
            {
                buffer.Endian = BitReader.Endians.Big;
            }

            while (buffer.ReadBoolean())
            {
                buffer.SeekBits(24);
            }

            buffer.SeekRemainingBitsInCurrentByte();
        }

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
                    Slot = buffer.ReadUBits(5),
                    Ping = buffer.ReadUBits(12),
                    Loss = buffer.ReadUBits(7)
                });
            }

            buffer.SeekRemainingBitsInCurrentByte();
        }

        public override void Write(BitWriter buffer)
        {
            foreach (Client client in Clients)
            {
                buffer.WriteBoolean(true);
                buffer.WriteUBits(client.Slot, 5);
                buffer.WriteUBits(client.Ping, 12);
                buffer.WriteUBits(client.Loss, 7);
            }

            buffer.WriteBoolean(false);
            buffer.PadRemainingBitsInCurrentByte();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Num clients: {0}", Clients.Count);
            // TODO
        }
    }
}
