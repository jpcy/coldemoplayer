using System;
using System.IO;

namespace CDP.HalfLife.Messages
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

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public short EntityId { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(2);
        }

        public override void Read(BitReader buffer)
        {
            EntityId = buffer.ReadShort();
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteShort(EntityId);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Entity ID: {0}", EntityId);
        }
    }
}
