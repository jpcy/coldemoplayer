using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;


namespace CDP.HalfLifeDemo.Messages
{
    public class SvcRoomType : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_roomtype; }
        }

        public override string Name
        {
            get { return "svc_roomtype"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public short RoomType { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(2);
        }

        public override void Read(BitReader buffer)
        {
            RoomType = buffer.ReadShort();
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteShort(RoomType);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Room type: {0}", RoomType);
        }
    }
}
