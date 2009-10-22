using System;
using System.IO;
using CDP.Core;

namespace CDP.Source.Messages
{
    public class NetTick : Message
    {
        public override MessageIds Id
        {
            get { return MessageIds.NET_Tick; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.NET_Tick; }
        }

        public override string Name
        {
            get { return "NET_Tick"; }
        }

        public int Tick { get; set; }
        public float? Timestamp { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(4);

            if (Demo.NetworkProtocol >= 14)
            {
                buffer.SeekBytes(4);
            }
        }

        public override void Read(BitReader buffer)
        {
            Tick = buffer.ReadInt();

            if (Demo.NetworkProtocol >= 14)
            {
                Timestamp = buffer.ReadFloat();
            }
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteInt(Tick);

            if (Timestamp.HasValue)
            {
                buffer.WriteFloat(Timestamp.Value);
            }
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Tick: {0}", Tick);
            log.WriteLine("Timestamp: {0}", Timestamp);
        }
    }
}
