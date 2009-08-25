using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
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

        public byte Index { get; set; }
        public string Map { get; set; }

        public override void Read(BitReader buffer)
        {
            Index = buffer.ReadByte();
            Map = buffer.ReadString();
        }

        public override byte[] Write()
        {
            BitWriter buffer = new BitWriter();
            buffer.WriteByte(Index);
            buffer.WriteString(Map);
            return buffer.Data;
        }

#if DEBUG
        public override void Log(StreamWriter log)
        {
            log.WriteLine("Index: {0}", Index);
            log.WriteLine("Map: {0}", Map);
        }
#endif
    }
}
