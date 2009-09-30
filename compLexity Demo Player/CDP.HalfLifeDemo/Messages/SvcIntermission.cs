using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcIntermission : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_intermission; }
        }

        public override string Name
        {
            get { return "svc_intermission"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public override void Skip(BitReader buffer)
        {
        }

        public override void Read(BitReader buffer)
        {
        }

        public override void Write(BitWriter buffer)
        {
        }

        public override void Log(StreamWriter log)
        {
        }
    }
}
