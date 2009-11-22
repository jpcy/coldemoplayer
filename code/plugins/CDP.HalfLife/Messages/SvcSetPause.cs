using System;
using System.IO;

namespace CDP.HalfLife.Messages
{
    public class SvcSetPause : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_setpause; }
        }

        public override string Name
        {
            get { return "svc_setpause"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public byte State { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(1);
        }

        public override void Read(BitReader buffer)
        {
            State = buffer.ReadByte();
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteByte(State);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("State: {0}", State);
        }
    }
}
