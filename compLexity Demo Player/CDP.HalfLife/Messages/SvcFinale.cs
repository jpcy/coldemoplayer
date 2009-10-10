using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLife.Messages
{
    public class SvcFinale : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_finale; }
        }

        public override string Name
        {
            get { return "svc_finale"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public string Value { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekString();
        }

        public override void Read(BitReader buffer)
        {
            Value = buffer.ReadString(); // Always empty.
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteString(Value);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Value: {0}", Value);
        }
    }
}
