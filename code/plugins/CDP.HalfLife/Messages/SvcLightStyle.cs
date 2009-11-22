using System;
using System.IO;

namespace CDP.HalfLife.Messages
{
    public class SvcLightStyle : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_lightstyle; }
        }

        public override string Name
        {
            get { return "svc_lightstyle"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public byte Index { get; set; }
        public string Map { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(1);
            buffer.SeekString();
        }

        public override void Read(BitReader buffer)
        {
            Index = buffer.ReadByte();
            Map = buffer.ReadString();
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteByte(Index);
            buffer.WriteString(Map);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Index: {0}", Index);
            log.WriteLine("Map: {0}", Map);
        }
    }
}
