using System;
using System.IO;
using BitReader = CDP.Core.BitReader;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcSetView : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_setview; }
        }

        public override string Name
        {
            get { return "svc_setview"; }
        }

        public short EntityId { get; set; }

        public override void Read(BitReader buffer)
        {
            EntityId = buffer.ReadShort();
        }

        public override byte[] Write()
        {
            return BitConverter.GetBytes(EntityId);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Entity ID: {0}", EntityId);
        }
    }
}
