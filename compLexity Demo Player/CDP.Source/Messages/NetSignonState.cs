using System;
using System.IO;
using CDP.Core;
using CDP.Core.Extensions;

namespace CDP.Source.Messages
{
    public class NetSignonState : Message
    {
        public override MessageIds Id
        {
            get { return MessageIds.NET_SignonState; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.NET_SignonState; }
        }

        public override string Name
        {
            get { return "NET_SignonState"; }
        }

        public byte State { get; set; }
        public int Count { get; set; }
        public int Unknown1 { get; set; }
        public byte[] Unknown2 { get; set; }
        public byte[] Unknown3 { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(5);

            if (Demo.NetworkProtocol >= 36)
            {
                buffer.SeekBytes(4);
                int length = buffer.ReadInt();
                buffer.SeekBytes(length);
                length = buffer.ReadInt();
                buffer.SeekBytes(length);
            }
        }

        public override void Read(BitReader buffer)
        {
            State = buffer.ReadByte();
            Count = buffer.ReadInt();

            if (Demo.NetworkProtocol >= 36)
            {
                Unknown1 = buffer.ReadInt();
                int uk2length = buffer.ReadInt();

                if (uk2length > 0)
                {
                    Unknown2 = buffer.ReadBytes(uk2length);
                }

                int uk3length = buffer.ReadInt();

                if (uk3length > 0)
                {
                    Unknown3 = buffer.ReadBytes(uk3length);
                }
            }
        }

        public override void Write(BitWriter buffer)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("State: {0}", State);
            log.WriteLine("Count: {0}", Count);

            if (Demo.NetworkProtocol >= 36)
            {
                log.WriteLine("Unknown1: {0}", Unknown1);
                log.WriteBytes(Unknown2);
                log.WriteBytes(Unknown3);
            }
        }
    }
}
